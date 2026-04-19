using SyncroApplicationLayer.DTOs;

namespace SyncroApplicationLayer.Interfaces;

public interface ITenantService
{
    Task<List<TenantDto>> GetAllAsync();
    Task<TenantDto?> GetByIdAsync(Guid id);
    Task<TenantDto> CreateAsync(CreateTenantDto dto);
    Task<TenantDto?> UpdateAsync(Guid id, UpdateTenantDto dto);
    Task<bool> DeleteAsync(Guid id);
}
