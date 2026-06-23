using SyncroApplicationLayer.DTOs;

namespace SyncroApplicationLayer.Interfaces;

public interface IDeviceSensorService
{
    Task<List<DeviceSensorDto>> GetByDeviceAsync(string deviceId);
    Task<DeviceSensorDto?> GetByIdAsync(string id);
    Task<DeviceSensorDto> InstallAsync(CreateDeviceSensorDto dto);
    Task<DeviceSensorDto?> UpdateAsync(string id, UpdateDeviceSensorDto dto);
    Task<bool> UpdateDisplayNameAsync(string id, string name);
    Task<bool> UpdateInchingAsync(string id, bool enabled, int widthMs);
    Task<bool> UninstallAsync(string id);
    Task SyncFromDeviceAsync(string deviceId, SensorConfigEnvelope envelope);
}
