using SyncroApplicationLayer.DTOs;

namespace SyncroApplicationLayer.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<List<TenantDto>> GetTenantsAsync(Guid userId);
    Task<UserDto> CreateAsync(CreateUserDto dto);
    Task<UserDto?> UpdateAsync(Guid id, UpdateUserDto dto);
    Task<bool> AddToTenantAsync(Guid userId, Guid tenantId);
    Task<bool> RemoveFromTenantAsync(Guid userId, Guid tenantId);
    Task<bool> DeleteAsync(Guid id);
}
