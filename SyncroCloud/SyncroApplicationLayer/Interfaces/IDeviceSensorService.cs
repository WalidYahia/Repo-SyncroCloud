using SyncroApplicationLayer.DTOs;

namespace SyncroApplicationLayer.Interfaces;

public interface IDeviceSensorService
{
    Task<List<DeviceSensorDto>> GetByDeviceAsync(Guid deviceId);
    Task<DeviceSensorDto?> GetByIdAsync(Guid deviceId, Guid sensorId);
    Task<DeviceSensorDto> InstallAsync(CreateDeviceSensorDto dto);
    Task<DeviceSensorDto?> UpdateAsync(Guid deviceId, Guid sensorId, UpdateDeviceSensorDto dto);
    Task<bool> UpdateLastReadingAsync(Guid deviceId, Guid sensorId, string json);
    Task<bool> UninstallAsync(Guid deviceId, Guid sensorId);
}
