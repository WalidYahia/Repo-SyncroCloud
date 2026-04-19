using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Data;
using SyncroInfraLayer.Entities;

namespace SyncroApplicationLayer.Services;

public class UserService(SyncroDbContext db) : IUserService
{
    public async Task<List<UserDto>> GetAllAsync() =>
        await db.Users.Select(u => ToDto(u)).ToListAsync();

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var u = await db.Users.FindAsync(id);
        return u is null ? null : ToDto(u);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        var user = new User { Id = Guid.NewGuid(), Email = dto.Email, FirstName = dto.FirstName, LastName = dto.LastName, CreatedAt = DateTime.UtcNow, IsActive = true };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return ToDto(user);
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return null;
        user.Email = dto.Email;
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.IsActive = dto.IsActive;
        await db.SaveChangesAsync();
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
        var user = await db.Users.FindAsync(id);
        if (user is null) return false;
        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return true;
    }

    private static UserDto ToDto(User u) => new(u.Id, u.Email, u.FirstName, u.LastName, u.CreatedAt, u.IsActive);
}
