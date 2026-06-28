namespace SyncroApplicationLayer.Interfaces;

public interface INotificationService
{
    Task SendSensorDataUpdatedAsync(string deviceId, string deviceSensorId, string? reading);
    Task SendDeviceStatusChangedAsync(string deviceId, string status);
    Task SendSensorConfigChangedAsync(string deviceId);
    Task SendScenarioConfigChangedAsync(string deviceId);
}
