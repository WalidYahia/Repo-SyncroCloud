using SyncroApplicationLayer.DTOs;

namespace SyncroApplicationLayer.Interfaces;

public interface ISensorService
{
    Task<List<SensorDto>> GetAllAsync();
    Task<SensorDto?> GetByIdAsync(Guid id);
    Task<SensorDto> CreateAsync(CreateSensorDto dto);
    Task<SensorDto?> UpdateAsync(Guid id, UpdateSensorDto dto);
    Task<bool> DeleteAsync(Guid id);
}
