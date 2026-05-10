namespace SyncroApplicationLayer.Interfaces;

public interface INotificationService
{
    Task SendSensorDataUpdatedAsync(string deviceId, Guid sensorId, string? reading);
    Task SendDeviceStatusChangedAsync(string deviceId, string status);
    Task SendSensorConfigChangedAsync(string deviceId);
}
