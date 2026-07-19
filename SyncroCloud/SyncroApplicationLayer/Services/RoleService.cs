using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Data;
using SyncroInfraLayer.Entities;
using SyncroInfraLayer.Identity;

namespace SyncroApplicationLayer.Services;

public class RoleService(RoleManager<AppRole> roleManager, SyncroDbContext db) : IRoleService
{
    public async Task<List<PrivilegeDto>> GetAllPrivilegesAsync() =>
        await db.Privileges
            .Select(p => new PrivilegeDto(p.Id, p.Code, p.Name))
            .ToListAsync();

    public async Task<List<RoleDetailDto>> GetAllRolesAsync()
    {
        var roles = await roleManager.Roles.ToListAsync();
        var result = new List<RoleDetailDto>();

        foreach (var role in roles)
        {
            var privileges = await db.RolePrivileges
                .Where(rp => rp.RoleId == role.Id)
                .Select(rp => new PrivilegeDto(rp.Privilege.Id, rp.Privilege.Code, rp.Privilege.Name))
                .ToListAsync();

            result.Add(new RoleDetailDto(role.Id, role.Name!, privileges));
        }

        return result;
    }

    public async Task<RoleDetailDto?> GetRoleAsync(Guid roleId)
    {
        var role = await roleManager.FindByIdAsync(roleId.ToString());
        if (role is null) return null;

        var privileges = await db.RolePrivileges
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => new PrivilegeDto(rp.Privilege.Id, rp.Privilege.Code, rp.Privilege.Name))
            .ToListAsync();

        return new RoleDetailDto(role.Id, role.Name!, privileges);
    }

    public async Task<(bool Success, RoleDetailDto? Role, string? Error)> CreateRoleAsync(CreateRoleDto dto)
    {
        if (await roleManager.RoleExistsAsync(dto.Name))
            return (false, null, $"Role '{dto.Name}' already exists.");

        var role = new AppRole(dto.Name);
        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
            return (false, null, string.Join("; ", result.Errors.Select(e => e.Description)));

        var validPrivilegeIds = await db.Privileges
            .Where(p => dto.PrivilegeIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync();

        foreach (var privId in validPrivilegeIds)
            db.RolePrivileges.Add(new RolePrivilege { RoleId = role.Id, PrivilegeId = privId });

        await db.SaveChangesAsync();

        var roleDetail = await GetRoleAsync(role.Id);
        return (true, roleDetail, null);
    }

    public async Task<bool> UpdatePrivilegesAsync(Guid roleId, UpdateRolePrivilegesDto dto)
    {
        var role = await roleManager.FindByIdAsync(roleId.ToString());
        if (role is null) return false;

        var existing = await db.RolePrivileges.Where(rp => rp.RoleId == roleId).ToListAsync();
        db.RolePrivileges.RemoveRange(existing);

        var validPrivilegeIds = await db.Privileges
            .Where(p => dto.PrivilegeIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync();

        foreach (var privId in validPrivilegeIds)
            db.RolePrivileges.Add(new RolePrivilege { RoleId = roleId, PrivilegeId = privId });

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<string>> GetUserPrivilegeCodesAsync(Guid userId)
    {
        var userRoleIds = await db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        if (userRoleIds.Count == 0) return [];

        return await db.RolePrivileges
            .Where(rp => userRoleIds.Contains(rp.RoleId))
            .Select(rp => rp.Privilege.Code)
            .Distinct()
            .ToListAsync();
    }

    public async Task<bool> DeleteRoleAsync(Guid roleId)
    {
        var role = await roleManager.FindByIdAsync(roleId.ToString());
        if (role is null) return false;

        // Prevent deleting built-in roles
        if (role.Name is AppRoles.SuperAdmin or AppRoles.TenantAdmin or AppRoles.User)
            return false;

        var result = await roleManager.DeleteAsync(role);
        return result.Succeeded;
    }
}
