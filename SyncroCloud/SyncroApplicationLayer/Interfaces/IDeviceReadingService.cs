using SyncroApplicationLayer.DTOs;

namespace SyncroApplicationLayer.Interfaces;

public interface IDeviceReadingService
{
    Task<List<DeviceReadingDto>> GetAsync(string deviceSensorId, DateTime? from, DateTime? to);
    Task<DeviceReadingDto> AddAsync(CreateDeviceReadingDto dto);
}
