using SyncroApplicationLayer.DTOs;
using SyncroInfraLayer.Enums;

namespace SyncroApplicationLayer.Interfaces;

public interface IDeviceService
{
    Task<List<DeviceDto>> GetAllAsync();
    Task<List<DeviceDto>> GetByTenantAsync(Guid tenantId);
    Task<DeviceDto?> GetByIdAsync(string id);
    Task<DeviceDto> CreateAsync(CreateDeviceDto dto);
    Task<DeviceDto?> UpdateAsync(string id, UpdateDeviceDto dto);
    Task<bool> UpdateStatusAsync(string id, DeviceStatus status);
    Task<bool> DeleteAsync(string id);
}
