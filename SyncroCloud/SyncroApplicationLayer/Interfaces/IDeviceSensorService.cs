using SyncroApplicationLayer.DTOs;

namespace SyncroApplicationLayer.Interfaces;

public interface IDeviceSensorService
{
    Task<List<DeviceSensorDto>> GetByDeviceAsync(Guid deviceId);
    Task<DeviceSensorDto?> GetByIdAsync(long id);
    Task<DeviceSensorDto> InstallAsync(CreateDeviceSensorDto dto);
    Task<DeviceSensorDto?> UpdateAsync(long id, UpdateDeviceSensorDto dto);
    Task<bool> UpdateLastReadingAsync(Guid deviceId, Guid sensorId, string json);
    Task<bool> UninstallAsync(long id);
}
