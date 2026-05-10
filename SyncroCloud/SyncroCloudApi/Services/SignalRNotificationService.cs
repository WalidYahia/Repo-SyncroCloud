using Microsoft.AspNetCore.SignalR;
using SyncroApplicationLayer.Interfaces;
using SyncroCloudApi.Hubs;

namespace SyncroCloudApi.Services;

public class SignalRNotificationService(IHubContext<SyncroHub> hub) : INotificationService
{
    public Task SendSensorDataUpdatedAsync(string deviceId, Guid sensorId, string? reading) =>
        hub.Clients.Group($"device_{deviceId}")
           .SendAsync("SensorDataUpdated", new { deviceId, sensorId, reading });

    public Task SendDeviceStatusChangedAsync(string deviceId, string status) =>
        hub.Clients.Group($"device_{deviceId}")
           .SendAsync("DeviceStatusChanged", new { deviceId, status });

    public Task SendSensorConfigChangedAsync(string deviceId) =>
        hub.Clients.Group($"device_{deviceId}")
           .SendAsync("SensorConfigChanged", new { deviceId });
}
