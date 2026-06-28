using Microsoft.AspNetCore.SignalR;
using SyncroApplicationLayer.Interfaces;
using SyncroCloudApi.Hubs;

namespace SyncroCloudApi.Services;

public class SignalRNotificationService(IHubContext<SyncroHub> hub) : INotificationService
{
    public Task SendSensorDataUpdatedAsync(string deviceId, string deviceSensorId, string? reading) =>
        hub.Clients.Group($"device_{deviceId}")
           .SendAsync("SensorDataUpdated", new { deviceId, deviceSensorId, reading });

    public Task SendDeviceStatusChangedAsync(string deviceId, string status) =>
        hub.Clients.Group($"device_{deviceId}")
           .SendAsync("DeviceStatusChanged", new { deviceId, status });

    public Task SendSensorConfigChangedAsync(string deviceId) =>
        hub.Clients.Group($"device_{deviceId}")
           .SendAsync("SensorConfigChanged", new { deviceId });

    public Task SendScenarioConfigChangedAsync(string deviceId) =>
        hub.Clients.Group($"device_{deviceId}")
           .SendAsync("ScenarioConfigChanged", new { deviceId });
}
