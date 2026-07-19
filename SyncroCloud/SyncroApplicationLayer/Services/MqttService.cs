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
    ILogger<MqttService> logger,
    INotificationService notifier) : BackgroundService, IMqttService
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
                    .WithTopicFilter(f => f.WithTopic(MqttHelper.GetWildcardTopic(MqttTopics.DeviceSensorConfig)).WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
                    .WithTopicFilter(f => f.WithTopic(MqttHelper.GetWildcardTopic(MqttTopics.DeviceUserScenario)).WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
                    .WithTopicFilter(f => f.WithTopic(MqttHelper.GetHubWildcardTopic(MqttTopics.RemoteAction_Ack)).WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
                    .WithTopicFilter(f => f.WithTopic(MqttHelper.GetWildcardReadingsTopic(MqttTopics.Readings)).WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
                    .WithTopicFilter(f => f.WithTopic(MqttHelper.GetWildcardTopic(MqttTopics.Heartbeat)).WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce))
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

        try
        {
            using var scope = scopeFactory.CreateScope();

            // Syncro/{deviceId}/Readings/{deviceSensorId}
            if (MqttHelper.TryParseReading(topic, out var deviceId, out var deviceSensorId))
            {
                var readingService = scope.ServiceProvider.GetRequiredService<IDeviceReadingService>();
                var logService     = scope.ServiceProvider.GetRequiredService<IDeviceActionLogService>();

                await readingService.AddAsync(new CreateDeviceReadingDto(deviceId, deviceSensorId, payload));

                await logService.LogAsync(new CreateDeviceActionLogDto(
                    deviceId, deviceSensorId, "StateChanged", "Device", "Ok"));

                await notifier.SendSensorDataUpdatedAsync(deviceId, deviceSensorId, payload);
                logger.LogDebug("Stored reading for device {DeviceId} sensor {DeviceSensorId}", deviceId, deviceSensorId);
            }
            // Syncro/{deviceId}/status
            else if (MqttHelper.TryParseDeviceStatus(topic, out var statusDeviceId))
            {
                var deviceService = scope.ServiceProvider.GetRequiredService<IDeviceService>();
                var statusValue   = payload.Trim('"');
                var status        = statusValue.Equals("online", StringComparison.OrdinalIgnoreCase)
                    ? DeviceStatus.Online
                    : DeviceStatus.Offline;

                await deviceService.UpdateStatusAsync(statusDeviceId, status);
                await notifier.SendDeviceStatusChangedAsync(statusDeviceId, statusValue);
                logger.LogInformation("Device {DeviceId} is now {Status}", statusDeviceId, status);
            }
            // Syncro/{deviceId}/Heartbeat
            else if (MqttHelper.TryParseDeviceTopic(topic, MqttTopics.Heartbeat, out var heartbeatDeviceId))
            {
                var deviceService = scope.ServiceProvider.GetRequiredService<IDeviceService>();

                // Store the cloud-received time as last-seen — it is authoritative and
                // immune to hub clock drift (payload localTime is used only for diagnostics).
                var cloudReceivedAt = DateTime.UtcNow;
                var updated = await deviceService.UpdateLastSeenAsync(heartbeatDeviceId, cloudReceivedAt);

                if (!updated)
                    logger.LogWarning("Heartbeat from unknown device {DeviceId}", heartbeatDeviceId);
                else
                    logger.LogDebug("Heartbeat from {DeviceId} — last-seen updated", heartbeatDeviceId);
            }
            // Syncro/{deviceId}/DeviceSensorConfig
            else if (MqttHelper.TryParseDeviceTopic(topic, MqttTopics.DeviceSensorConfig, out var configDeviceId))
            {
                var envelope = JsonSerializer.Deserialize<SensorConfigEnvelope>(payload, _caseInsensitive);
                if (envelope is null)
                {
                    logger.LogWarning("Received null or unparseable SensorConfigEnvelope from device {DeviceId}", configDeviceId);
                    return;
                }

                var sensorService = scope.ServiceProvider.GetRequiredService<IDeviceSensorService>();
                await sensorService.SyncFromDeviceAsync(configDeviceId, envelope);
                await notifier.SendSensorConfigChangedAsync(configDeviceId);
                logger.LogInformation("Received sensor config v{Version} from device {DeviceId} ({Count} sensors)",
                    envelope.ConfigVersion, configDeviceId, envelope.Sensors.Count);
            }
            // Syncro/{deviceId}/DeviceUserScenario
            else if (MqttHelper.TryParseDeviceTopic(topic, MqttTopics.DeviceUserScenario, out var scenarioDeviceId))
            {
                var envelope = JsonSerializer.Deserialize<ScenarioConfigEnvelope>(payload, _caseInsensitive);
                if (envelope is null)
                {
                    logger.LogWarning("Received null or unparseable ScenarioConfigEnvelope from device {DeviceId}", scenarioDeviceId);
                    return;
                }

                var scenarioService = scope.ServiceProvider.GetRequiredService<IDeviceScenarioService>();
                await scenarioService.SyncFromDeviceAsync(scenarioDeviceId, envelope);
                await notifier.SendScenarioConfigChangedAsync(scenarioDeviceId);
                logger.LogInformation("Received scenario config v{Version} from device {DeviceId} ({Count} scenarios)",
                    envelope.ConfigVersion, scenarioDeviceId, envelope.Scenarios.Count);
            }
            // Syncro/{hubId}/RemoteAction_Ack
            else if (MqttHelper.TryParseDeviceTopic(topic, MqttTopics.RemoteAction_Ack, out var hubId))
            {
                var ack = JsonSerializer.Deserialize<RemoteActionAckDto>(payload, _caseInsensitive);
                if (ack is not null && _pendingAcks.TryRemove(ack.RequestId, out var tcs))
                    tcs.TrySetResult(ack);
                else
                    logger.LogDebug("Received unmatched RemoteAction_Ack from hub {HubId}", hubId);
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
            .WithTopic(MqttHelper.GetCommandTopic(deviceId, action))
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
        => SendRemoteActionAsync(hubId, JsonCommandType.InchingOn, new EnableInchingPayload(installedSensorId, unitId, inchingTimeInMs), ct);

    public Task<RemoteActionAckDto> DisableInchingAsync(string hubId, string installedSensorId, string unitId, CancellationToken ct = default)
        => SendRemoteActionAsync(hubId, JsonCommandType.InchingOff, new DisableInchingPayload(installedSensorId, unitId), ct);

    public Task<RemoteActionAckDto> UpdateUnitNameAsync(string hubId, string installedSensorId, string name, CancellationToken ct = default)
        => SendRemoteActionAsync(hubId, JsonCommandType.UpdateUnitName, new UpdateUnitNamePayload(installedSensorId, name), ct);

    public Task<RemoteActionAckDto> SaveScenarioAsync(string hubId, MqttUserScenarioDto scenario, CancellationToken ct = default)
        => SendRemoteActionAsync(hubId, JsonCommandType.SaveUSerScenario, new SaveScenarioPayload(scenario), ct);

    public Task<RemoteActionAckDto> DeleteScenarioAsync(string hubId, string scenarioId, CancellationToken ct = default)
        => SendRemoteActionAsync(hubId, JsonCommandType.DeleteUSerScenario, new DeleteScenarioPayload(new DeleteScenarioIdDto(scenarioId)), ct);

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
            .WithTopic(MqttHelper.GetMqttTopic(MqttTopics.RemoteAction, hubId))
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
