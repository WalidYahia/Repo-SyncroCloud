using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Data;
using SyncroInfraLayer.Entities;
using SyncroInfraLayer.Identity;

namespace SyncroApplicationLayer.Services;

public class UserService(UserManager<AppUser> userManager, SyncroDbContext db) : IUserService
{
    public async Task<List<UserDto>> GetAllAsync() =>
        await userManager.Users.Select(u => ToDto(u)).ToListAsync();

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var u = await userManager.FindByIdAsync(id.ToString());
        return u is null ? null : ToDto(u);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        await userManager.CreateAsync(user);
        return ToDto(user);
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return null;
        user.Email = dto.Email;
        user.UserName = dto.Email;
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.IsActive = dto.IsActive;
        await userManager.UpdateAsync(user);
        return ToDto(user);
    }

    public async Task<List<TenantDto>> GetTenantsAsync(Guid userId) =>
        await db.TenantUsers
            .Where(tu => tu.UserId == userId)
            .Select(tu => new TenantDto(tu.Tenant.Id, tu.Tenant.Name, tu.Tenant.CreatedAt, tu.Tenant.IsActive))
            .ToListAsync();

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

    private static UserDto ToDto(AppUser u) =>
        new(u.Id, u.Email!, u.FirstName, u.LastName, u.CreatedAt, u.IsActive);
}
