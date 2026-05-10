using SyncroApplicationLayer.DTOs;

namespace SyncroApplicationLayer.Interfaces;

public interface IDeviceActionLogService
{
    Task LogAsync(CreateDeviceActionLogDto dto);
    Task<List<DeviceActionLogDto>> GetByDeviceAsync(string deviceId,  int limit = 100);
    Task<List<DeviceActionLogDto>> GetBySensorAsync(string installedSensorId, int limit = 100);
}
