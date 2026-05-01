using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Enums;
using SyncroInfraLayer.Helpers;

namespace SyncroApplicationLayer.Services;

public class MqttService(
    IServiceScopeFactory scopeFactory,
    IConfiguration config,
    ILogger<MqttService> logger) : BackgroundService, IMqttService
{
    private IMqttClient _client = null!;
    private MqttClientOptions _options = null!;
    private TaskCompletionSource _disconnectedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly ConcurrentDictionary<string, TaskCompletionSource<RemoteActionAckDto>> _pendingAcks = new();

    private static readonly JsonSerializerOptions _caseInsensitive = new() { PropertyNameCaseInsensitive = true };

    private const int AckTimeoutSeconds = 30;

    public bool IsConnected => _client?.IsConnected ?? false;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();
        _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        _client.DisconnectedAsync += args =>
        {
            _disconnectedTcs.TrySetResult();
            foreach (var (_, tcs) in _pendingAcks)
                tcs.TrySetCanceled();
            _pendingAcks.Clear();
            return Task.CompletedTask;
        };

        var optionsBuilder = new MqttClientOptionsBuilder()
            .WithTcpServer(config["Mqtt:Broker"] ?? "localhost", config.GetValue<int>("Mqtt:Port", 1883))
            .WithClientId(config["Mqtt:ClientId"] ?? "SyncroCloud")
            .WithCredentials(config["Mqtt:Username"], config["Mqtt:Password"])
            .WithCleanSession();

        if (config.GetValue<bool>("Mqtt:UseTls"))
            optionsBuilder.WithTlsOptions(o => o.UseTls());

        _options = optionsBuilder.Build();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _client.ConnectAsync(_options, stoppingToken);
                logger.LogInformation("MQTT connected to {Broker}:{Port}", config["Mqtt:Broker"], config.GetValue<int>("Mqtt:Port", 1883));

                var subscribeOptions = factory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(f => f.WithTopic(MqttHelper.SensorDataWildcard).WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
                    .WithTopicFilter(f => f.WithTopic(MqttHelper.DeviceStatusWildcard).WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
                    .WithTopicFilter(f => f.WithTopic(MqttHelper.GetWildcardTopic(MqttTopics.DeviceSensorConfig)).WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
                    .WithTopicFilter(f => f.WithTopic($"+/{MqttTopics.RemoteAction_Ack}").WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
                    .Build();

                await _client.SubscribeAsync(subscribeOptions, stoppingToken);
                logger.LogInformation("MQTT subscribed to device topics");

                _disconnectedTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                using (stoppingToken.Register(() => _disconnectedTcs.TrySetCanceled()))
                    await _disconnectedTcs.Task;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "MQTT error — reconnecting in 5s");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            finally
            {
                if (_client.IsConnected)
                    await _client.DisconnectAsync();
            }
        }
    }

    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        var topic = e.ApplicationMessage.Topic;
        var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
        var parts = topic.Split('/');

        try
        {
            using var scope = scopeFactory.CreateScope();

            // syncro/{deviceId}/sensors/{sensorId}/data
            if (parts.Length == 5 && parts[0] == "Syncro" && parts[2] == "sensors" && parts[4] == "data")
            {
                var deviceId = parts[1];
                if (!Guid.TryParse(parts[3], out var sensorId))
                    return;

                var readingService = scope.ServiceProvider.GetRequiredService<IDeviceReadingService>();
                var sensorService = scope.ServiceProvider.GetRequiredService<IDeviceSensorService>();

                await readingService.AddAsync(new CreateDeviceReadingDto(deviceId, sensorId, DateTime.UtcNow, payload));
                await sensorService.UpdateLastReadingAsync(deviceId, sensorId, payload);

                logger.LogDebug("Stored reading for device {DeviceId} sensor {SensorId}", deviceId, sensorId);
            }
            // syncro/{deviceId}/status
            else if (parts.Length == 3 && parts[0] == "Syncro" && parts[2] == "status")
            {
                var deviceId = parts[1];

                var deviceService = scope.ServiceProvider.GetRequiredService<IDeviceService>();
                var status = payload.Trim('"').Equals("online", StringComparison.OrdinalIgnoreCase)
                    ? DeviceStatus.Online
                    : DeviceStatus.Offline;

                await deviceService.UpdateStatusAsync(deviceId, status);
                logger.LogInformation("Device {DeviceId} is now {Status}", deviceId, status);
            }
            // Syncro/{deviceId}/DeviceSensorConfig  — device pushes its full sensor list
            else if (parts.Length == 3 && parts[0] == "Syncro" && parts[2] == MqttTopics.DeviceSensorConfig.ToString())
            {
                var deviceId = parts[1];
                var sensors  = JsonSerializer.Deserialize<List<DeviceSensorSyncDto>>(payload, _caseInsensitive) ?? [];

                var sensorService = scope.ServiceProvider.GetRequiredService<IDeviceSensorService>();
                await sensorService.SyncFromDeviceAsync(deviceId, sensors);
                logger.LogInformation("Synced {Count} sensors from device {DeviceId}", sensors.Count, deviceId);
            }
            // {hubId}/RemoteAction_Ack  — hub acknowledges a remote command
            else if (parts.Length == 2 && parts[1] == MqttTopics.RemoteAction_Ack.ToString())
            {
                var ack = JsonSerializer.Deserialize<RemoteActionAckDto>(payload, _caseInsensitive);
                if (ack is not null && _pendingAcks.TryRemove(ack.RequestId, out var tcs))
                    tcs.TrySetResult(ack);
                else
                    logger.LogDebug("Received unmatched RemoteAction_Ack from hub {HubId}", parts[0]);
            }
            else
            {
                logger.LogDebug("Unhandled topic: {Topic}", topic);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing MQTT message on topic {Topic}", topic);
        }
    }

    public async Task PublishAsync(string topic, object payload, bool retainFlag, CancellationToken ct = default)
    {
        if (!_client.IsConnected)
        {
            logger.LogWarning("Cannot publish to {Topic} — MQTT client not connected", topic);
            return;
        }

        var json    = JsonSerializer.Serialize(payload);
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(json)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag(retainFlag)
            .Build();

        await _client.PublishAsync(message, ct);
        logger.LogDebug("Published to {Topic}", topic);
    }

    public async Task PublishCommandAsync(string deviceId, string action, object payload, CancellationToken ct = default)
    {
        if (!_client.IsConnected)
        {
            logger.LogWarning("Cannot publish — MQTT client not connected");
            return;
        }

        var json    = JsonSerializer.Serialize(payload);
        var message = new MqttApplicationMessageBuilder()
            .WithTopic($"syncro/{deviceId}/commands/{action}")
            .WithPayload(json)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag(false)
            .Build();

        await _client.PublishAsync(message, ct);
        logger.LogDebug("Published command '{Action}' to device {DeviceId}", action, deviceId);
    }

    // ── Remote Actions ────────────────────────────────────────

    public Task<RemoteActionAckDto> TurnOffUnitAsync(string hubId, string installedSensorId, CancellationToken ct = default)
        => SendRemoteActionAsync(hubId, JsonCommandType.TurnOff, new TurnUnitPayload(installedSensorId), ct);

    public Task<RemoteActionAckDto> TurnOnUnitAsync(string hubId, string installedSensorId, CancellationToken ct = default)
        => SendRemoteActionAsync(hubId, JsonCommandType.TurnOn, new TurnUnitPayload(installedSensorId), ct);

    public Task<RemoteActionAckDto> EnableInchingAsync(string hubId, string installedSensorId, string unitId, int inchingTimeInMs, CancellationToken ct = default)
        => SendRemoteActionAsync(hubId, JsonCommandType.EnableInching, new EnableInchingPayload(installedSensorId, unitId, inchingTimeInMs), ct);

    public Task<RemoteActionAckDto> DisableInchingAsync(string hubId, string installedSensorId, string unitId, CancellationToken ct = default)
        => SendRemoteActionAsync(hubId, JsonCommandType.DisableInching, new DisableInchingPayload(installedSensorId, unitId), ct);

    public Task<RemoteActionAckDto> UpdateUnitNameAsync(string hubId, string installedSensorId, string name, CancellationToken ct = default)
        => SendRemoteActionAsync(hubId, JsonCommandType.UpdateUnitName, new UpdateUnitNamePayload(installedSensorId, name), ct);

    public Task<RemoteActionAckDto> SaveScenarioAsync(string hubId, MqttUserScenarioDto scenario, CancellationToken ct = default)
        => SendRemoteActionAsync(hubId, JsonCommandType.SaveScenario, new SaveScenarioPayload(scenario), ct);

    public Task<RemoteActionAckDto> DeleteScenarioAsync(string hubId, string scenarioId, CancellationToken ct = default)
        => SendRemoteActionAsync(hubId, JsonCommandType.DeleteScenario, new DeleteScenarioPayload(new DeleteScenarioIdDto(scenarioId)), ct);

    private async Task<RemoteActionAckDto> SendRemoteActionAsync<TPayload>(
        string hubId, JsonCommandType commandType, TPayload commandPayload, CancellationToken ct)
    {
        if (!_client.IsConnected)
            throw new InvalidOperationException("MQTT client is not connected");

        var requestId = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{config["Mqtt:ClientId"] ?? "SyncroCloud"}";
        var tcs = new TaskCompletionSource<RemoteActionAckDto>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingAcks[requestId] = tcs;

        var payloadElement = JsonSerializer.SerializeToElement(commandPayload);
        var envelope = new RemoteActionEnvelope(requestId, commandType, payloadElement);
        var json = JsonSerializer.Serialize(envelope);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic($"{hubId}/{MqttTopics.RemoteAction}")
            .WithPayload(json)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag(false)
            .Build();

        await _client.PublishAsync(message, ct);
        logger.LogDebug("Published {CommandType} to hub {HubId}, requestId={RequestId}", commandType, hubId, requestId);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(AckTimeoutSeconds));
        try
        {
            return await tcs.Task.WaitAsync(linkedCts.Token);
        }
        finally
        {
            _pendingAcks.TryRemove(requestId, out _);
        }
    }
}
