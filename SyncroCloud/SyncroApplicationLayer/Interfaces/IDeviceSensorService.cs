using SyncroApplicationLayer.DTOs;

namespace SyncroApplicationLayer.Interfaces;

public interface IDeviceSensorService
{
    Task<List<DeviceSensorDto>> GetByDeviceAsync(string deviceId);
    Task<DeviceSensorDto?> GetByIdAsync(string id);
    Task<DeviceSensorDto> InstallAsync(CreateDeviceSensorDto dto);
    Task<DeviceSensorDto?> UpdateAsync(string id, UpdateDeviceSensorDto dto);
    Task<bool> UpdateLastReadingAsync(string deviceId, Guid sensorId, string json);
    Task<bool> UninstallAsync(string id);
}
