using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Data;
using SyncroInfraLayer.Entities;
using SyncroInfraLayer.Enums;
using SyncroInfraLayer.Identity;

namespace SyncroApplicationLayer.Services;

public class UserService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, SyncroDbContext db) : IUserService
{
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public async Task<List<UserDto>> GetAllAsync()
    {
        var users = await userManager.Users.ToListAsync();
        var rolesByUser = await GetRolesByUserIdAsync(users.Select(u => u.Id));
        return users.Select(u => ToDto(u, rolesByUser.GetValueOrDefault(u.Id, []))).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var u = await userManager.FindByIdAsync(id.ToString());
        if (u is null) return null;
        var roles = await userManager.GetRolesAsync(u);
        return ToDto(u, roles);
    }

    public async Task<List<UserDto>> GetByTenantAsync(Guid tenantId)
    {
        var users = await db.TenantUsers
            .Where(tu => tu.TenantId == tenantId)
            .Select(tu => tu.User)
            .ToListAsync();

        var rolesByUser = await GetRolesByUserIdAsync(users.Select(u => u.Id));
        return users.Select(u => ToDto(u, rolesByUser.GetValueOrDefault(u.Id, []))).ToList();
    }

    public async Task<List<Guid>> GetTenantAdminIdsAsync(Guid tenantId)
    {
        var tenantUserIds = await db.TenantUsers
            .Where(tu => tu.TenantId == tenantId)
            .Select(tu => tu.UserId)
            .ToListAsync();

        if (tenantUserIds.Count == 0) return [];

        var tenantAdminRoleId = await db.Roles
            .Where(r => r.Name == AppRoles.TenantAdmin)
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        return await db.UserRoles
            .Where(ur => ur.RoleId == tenantAdminRoleId && tenantUserIds.Contains(ur.UserId))
            .Select(ur => ur.UserId)
            .ToListAsync();
    }

    public async Task<List<RoleDto>> GetRolesAsync() =>
        await roleManager.Roles.Select(r => new RoleDto(r.Id, r.Name!)).ToListAsync();

    public async Task<List<TenantDto>> GetTenantsAsync(Guid userId) =>
        await db.TenantUsers
            .Where(tu => tu.UserId == userId)
            .Select(tu => new TenantDto(tu.Tenant.Id, tu.Tenant.Name, tu.Tenant.CreatedAt, tu.Tenant.IsActive))
            .ToListAsync();

    // ── Provisioning (shared by self-registration and admin user-creation) ──

    public async Task<(bool Success, UserDto? User, IEnumerable<string> Errors)> ProvisionAsync(ProvisionUserRequest request)
    {
        var tenantExists = await db.Tenants.AnyAsync(t => t.Id == request.TenantId && t.IsActive);
        if (!tenantExists)
            return (false, null, ["Tenant not found or inactive."]);

        var role = await roleManager.FindByIdAsync(request.RoleId.ToString());
        if (role is null)
            return (false, null, ["Role not found."]);

        var user = new AppUser
        {
            Id          = Guid.NewGuid(),
            UserName    = request.PhoneNumber,
            Email       = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email,
            PhoneNumber = request.PhoneNumber,
            FirstName   = request.FirstName,
            LastName    = request.LastName,
            CreatedAt   = DateTime.UtcNow,
            IsActive    = true
        };

        // Identity UserManager shares SyncroDbContext, so its internal SaveChanges participate in this transaction.
        await using var tx = await db.Database.BeginTransactionAsync();

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return (false, null, result.Errors.Select(e => e.Description)); // auto-rolled-back by await using

        await userManager.AddToRoleAsync(user, role.Name!);

        db.TenantUsers.Add(new TenantUser { TenantId = request.TenantId, UserId = user.Id, JoinedAt = DateTime.UtcNow });

        if (role.Name == AppRoles.SuperAdmin)
            await GrantFullControlAccessAsync(user.Id, tenantIds: null);
        else if (role.Name == AppRoles.TenantAdmin)
            await GrantFullControlAccessAsync(user.Id, [request.TenantId]);

        await db.SaveChangesAsync();
        await tx.CommitAsync();

        return (true, ToDto(user, [role.Name!]), []);
    }

    public async Task<UserDto?> UpdateRoleAsync(Guid userId, Guid roleId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return null;

        var role = await roleManager.FindByIdAsync(roleId.ToString());
        if (role is null) return null;

        await using var tx = await db.Database.BeginTransactionAsync();

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
            await userManager.RemoveFromRolesAsync(user, currentRoles);
        await userManager.AddToRoleAsync(user, role.Name!);

        if (role.Name == AppRoles.SuperAdmin)
        {
            await GrantFullControlAccessAsync(user.Id, tenantIds: null);
        }
        else if (role.Name == AppRoles.TenantAdmin)
        {
            var tenantIds = (await GetTenantsAsync(user.Id)).Select(t => t.Id).ToList();
            if (tenantIds.Count > 0)
                await GrantFullControlAccessAsync(user.Id, tenantIds);
        }

        await db.SaveChangesAsync();
        await tx.CommitAsync();

        return ToDto(user, [role.Name!]);
    }

    /// <summary>
    /// Grants the user Control access to every sensor on the target devices (all devices when
    /// <paramref name="tenantIds"/> is null, otherwise scoped to those tenants). Existing links are
    /// upgraded to the full sensor set rather than skipped, so this is also safe to call on promotion.
    /// </summary>
    private async Task GrantFullControlAccessAsync(Guid userId, List<Guid>? tenantIds)
    {
        var devices = db.Devices.AsQueryable();
        if (tenantIds is not null)
            devices = devices.Where(d => tenantIds.Contains(d.TenantId));

        var deviceIds = await devices.Select(d => d.DeviceId).ToListAsync();
        if (deviceIds.Count == 0) return;

        var sensorConfigs = await db.DeviceConfigs
            .Where(c => deviceIds.Contains(c.DeviceId) && c.ConfigType == ConfigType.Sensor && c.UpdatedFrom == ConfigSource.Cloud)
            .ToDictionaryAsync(c => c.DeviceId, c => c.Config);

        var existingLinks = await db.DeviceUsers
            .Where(du => du.UserId == userId && deviceIds.Contains(du.DeviceId))
            .ToDictionaryAsync(du => du.DeviceId);

        foreach (var deviceId in deviceIds)
        {
            var sensorIds = sensorConfigs.TryGetValue(deviceId, out var config)
                ? (JsonSerializer.Deserialize<List<DeviceSensorSyncDto>>(config, _json) ?? []).Select(s => s.Id)
                : [];

            var permissionsJson = JsonSerializer.Serialize(
                sensorIds.Select(id => new SensorPermissionDto(id, SensorAccessLevel.Control)), _json);

            if (existingLinks.TryGetValue(deviceId, out var existing))
            {
                existing.SensorPermissions = permissionsJson;
            }
            else
            {
                db.DeviceUsers.Add(new DeviceUser
                {
                    DeviceId          = deviceId,
                    UserId            = userId,
                    LinkedAt          = DateTime.UtcNow,
                    SensorPermissions = permissionsJson
                });
            }
        }
    }

    public async Task<(bool Success, UserDto? User, IEnumerable<string> Errors)> CreateAsync(CreateUserDto dto)
    {
        var role = await roleManager.FindByIdAsync(dto.RoleId.ToString());
        if (role is null)
            return (false, null, ["Role not found."]);

        List<Guid> tenantIds;
        if (role.Name == AppRoles.SuperAdmin)
        {
            tenantIds = await db.Tenants.Where(t => t.IsActive).Select(t => t.Id).ToListAsync();
        }
        else
        {
            if (dto.TenantIds.Count == 0)
                return (false, null, ["At least one tenant must be specified."]);

            var validCount = await db.Tenants.CountAsync(t => dto.TenantIds.Contains(t.Id) && t.IsActive);
            if (validCount != dto.TenantIds.Count)
                return (false, null, ["One or more tenants were not found or are inactive."]);

            tenantIds = dto.TenantIds;
        }

        var user = new AppUser
        {
            Id          = Guid.NewGuid(),
            UserName    = dto.PhoneNumber,
            Email       = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email,
            PhoneNumber = dto.PhoneNumber,
            FirstName   = dto.FirstName,
            LastName    = dto.LastName,
            CreatedAt   = DateTime.UtcNow,
            IsActive    = true
        };

        await using var tx = await db.Database.BeginTransactionAsync();

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return (false, null, result.Errors.Select(e => e.Description));

        await userManager.AddToRoleAsync(user, role.Name!);

        foreach (var tenantId in tenantIds)
            db.TenantUsers.Add(new TenantUser { TenantId = tenantId, UserId = user.Id, JoinedAt = DateTime.UtcNow });

        if (role.Name == AppRoles.SuperAdmin)
            await GrantFullControlAccessAsync(user.Id, tenantIds: null);
        else if (role.Name == AppRoles.TenantAdmin)
            await GrantFullControlAccessAsync(user.Id, tenantIds);

        await db.SaveChangesAsync();
        await tx.CommitAsync();

        return (true, ToDto(user, [role.Name!]), []);
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return null;
        user.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email;
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.IsActive = dto.IsActive;
        await userManager.UpdateAsync(user);

        var roles = await userManager.GetRolesAsync(user);
        return ToDto(user, roles);
    }

    public async Task<bool> AddToTenantAsync(Guid userId, Guid tenantId)
    {
        var exists = await db.TenantUsers.FindAsync(tenantId, userId);
        if (exists is not null) return false;
        db.TenantUsers.Add(new TenantUser { TenantId = tenantId, UserId = userId });
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveFromTenantAsync(Guid userId, Guid tenantId)
    {
        var tu = await db.TenantUsers.FindAsync(tenantId, userId);
        if (tu is null) return false;
        db.TenantUsers.Remove(tu);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return false;
        await userManager.DeleteAsync(user);
        return true;
    }

    private async Task<Dictionary<Guid, IReadOnlyList<string>>> GetRolesByUserIdAsync(IEnumerable<Guid> userIds)
    {
        var ids = userIds.ToList();
        if (ids.Count == 0) return [];

        var pairs = await (
            from ur in db.UserRoles
            where ids.Contains(ur.UserId)
            join r in db.Roles on ur.RoleId equals r.Id
            select new { ur.UserId, RoleName = r.Name! }
        ).ToListAsync();

        return pairs
            .GroupBy(p => p.UserId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<string>)g.Select(p => p.RoleName).ToList());
    }

    private static UserDto ToDto(AppUser u, IEnumerable<string> roles) =>
        new(u.Id, u.PhoneNumber!, u.Email, u.FirstName, u.LastName, u.CreatedAt, u.IsActive, roles.ToList());
}
