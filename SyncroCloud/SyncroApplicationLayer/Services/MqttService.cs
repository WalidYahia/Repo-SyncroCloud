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

namespace SyncroApplicationLayer.Services;

public class MqttService(
    IServiceScopeFactory scopeFactory,
    IConfiguration config,
    ILogger<MqttService> logger) : BackgroundService, IMqttService
{
    private IMqttClient _client = null!;
    private MqttClientOptions _options = null!;

    // Topic patterns:
    //   syncro/{deviceId}/sensors/{sensorId}/data  → incoming reading
    //   syncro/{deviceId}/status                    → device online/offline
    //   syncro/{deviceId}/commands/{action}         → outgoing commands

    public bool IsConnected => _client?.IsConnected ?? false;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();
        _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;

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
                    .WithTopicFilter(f => f.WithTopic("syncro/+/sensors/+/data").WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
                    .WithTopicFilter(f => f.WithTopic("syncro/+/status").WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce))
                    .Build();

                await _client.SubscribeAsync(subscribeOptions, stoppingToken);
                logger.LogInformation("MQTT subscribed to device topics");

                await Task.Delay(Timeout.Infinite, stoppingToken);
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
            if (parts.Length == 5 && parts[0] == "syncro" && parts[2] == "sensors" && parts[4] == "data")
            {
                if (!Guid.TryParse(parts[1], out var deviceId) || !Guid.TryParse(parts[3], out var sensorId))
                    return;

                var readingService = scope.ServiceProvider.GetRequiredService<IDeviceReadingService>();
                var sensorService = scope.ServiceProvider.GetRequiredService<IDeviceSensorService>();

                await readingService.AddAsync(new CreateDeviceReadingDto(deviceId, sensorId, DateTime.UtcNow, payload));
                await sensorService.UpdateLastReadingAsync(deviceId, sensorId, payload);

                logger.LogDebug("Stored reading for device {DeviceId} sensor {SensorId}", deviceId, sensorId);
            }
            // syncro/{deviceId}/status
            else if (parts.Length == 3 && parts[0] == "syncro" && parts[2] == "status")
            {
                if (!Guid.TryParse(parts[1], out var deviceId)) return;

                var deviceService = scope.ServiceProvider.GetRequiredService<IDeviceService>();
                var status = payload.Trim('"').Equals("online", StringComparison.OrdinalIgnoreCase)
                    ? DeviceStatus.Online
                    : DeviceStatus.Offline;

                await deviceService.UpdateStatusAsync(deviceId, status);
                logger.LogInformation("Device {DeviceId} is now {Status}", deviceId, status);
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

    public async Task PublishCommandAsync(Guid deviceId, string action, object payload, CancellationToken ct = default)
    {
        if (!_client.IsConnected)
        {
            logger.LogWarning("Cannot publish — MQTT client not connected");
            return;
        }

        var json = JsonSerializer.Serialize(payload);
        var message = new MqttApplicationMessageBuilder()
            .WithTopic($"syncro/{deviceId}/commands/{action}")
            .WithPayload(json)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag(false)
            .Build();

        await _client.PublishAsync(message, ct);
        logger.LogDebug("Published command '{Action}' to device {DeviceId}", action, deviceId);
    }
}
