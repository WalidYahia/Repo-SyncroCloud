using SyncroApplicationLayer.DTOs;
using SyncroInfraLayer.Enums;

namespace SyncroApplicationLayer.Interfaces;

public interface IAlarmLookupService
{
    Task<List<AlarmLookupDto>> GetAllAsync();
    Task<List<AlarmLookupDto>> GetBySensorTypeAsync(SensorType type);
    Task<AlarmLookupDto?> GetByIdAsync(int id);
    Task<AlarmLookupDto> CreateAsync(CreateAlarmLookupDto dto);
    Task<AlarmLookupDto?> UpdateAsync(int id, UpdateAlarmLookupDto dto);
    Task<bool> DeleteAsync(int id);
}
