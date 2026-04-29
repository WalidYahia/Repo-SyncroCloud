using SyncroApplicationLayer.DTOs;

namespace SyncroApplicationLayer.Interfaces;

public interface IDeviceReadingService
{
    Task<List<DeviceReadingDto>> GetAsync(string deviceId, Guid sensorId, DateTime? from, DateTime? to);
    Task<DeviceReadingDto> AddAsync(CreateDeviceReadingDto dto);
}
