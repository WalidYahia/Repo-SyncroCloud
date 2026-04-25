using SyncroApplicationLayer.DTOs;
using SyncroInfraLayer.Enums;

namespace SyncroApplicationLayer.Interfaces;

public interface IDeviceService
{
    Task<List<DeviceDto>> GetAllAsync();
    Task<List<DeviceDto>> GetByTenantAsync(Guid tenantId);
    Task<DeviceDto?> GetByIdAsync(Guid id);
    Task<DeviceDto> CreateAsync(CreateDeviceDto dto);
    Task<DeviceDto?> UpdateAsync(Guid id, UpdateDeviceDto dto);
    Task<bool> UpdateStatusAsync(Guid id, DeviceStatus status);
    Task<bool> DeleteAsync(Guid id);
}
