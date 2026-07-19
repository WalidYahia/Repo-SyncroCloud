using SyncroApplicationLayer.DTOs;

namespace SyncroApplicationLayer.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<List<UserDto>> GetByTenantAsync(Guid tenantId);
    Task<List<Guid>> GetTenantAdminIdsAsync(Guid tenantId);
    Task<List<TenantDto>> GetTenantsAsync(Guid userId);
    Task<List<RoleDto>> GetRolesAsync();
    Task<(bool Success, UserDto? User, IEnumerable<string> Errors)> ProvisionAsync(ProvisionUserRequest request);
    Task<(bool Success, UserDto? User, IEnumerable<string> Errors)> CreateAsync(CreateUserDto dto);
    Task<UserDto?> UpdateAsync(Guid id, UpdateUserDto dto);
    Task<UserDto?> UpdateRoleAsync(Guid userId, Guid roleId);
    Task<bool> AddToTenantAsync(Guid userId, Guid tenantId);
    Task<bool> RemoveFromTenantAsync(Guid userId, Guid tenantId);
    Task<bool> DeleteAsync(Guid id);
}
