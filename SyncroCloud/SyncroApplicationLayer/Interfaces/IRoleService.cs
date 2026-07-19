using SyncroApplicationLayer.DTOs;

namespace SyncroApplicationLayer.Interfaces;

public interface IRoleService
{
    Task<List<PrivilegeDto>> GetAllPrivilegesAsync();
    Task<List<RoleDetailDto>> GetAllRolesAsync();
    Task<RoleDetailDto?> GetRoleAsync(Guid roleId);
    Task<(bool Success, RoleDetailDto? Role, string? Error)> CreateRoleAsync(CreateRoleDto dto);
    Task<bool> UpdatePrivilegesAsync(Guid roleId, UpdateRolePrivilegesDto dto);
    Task<bool> DeleteRoleAsync(Guid roleId);
    Task<List<string>> GetUserPrivilegeCodesAsync(Guid userId);
}
