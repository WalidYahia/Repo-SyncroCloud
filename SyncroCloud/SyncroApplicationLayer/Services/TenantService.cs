using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Data;
using SyncroInfraLayer.Entities;

namespace SyncroApplicationLayer.Services;

public class TenantService(SyncroDbContext db) : ITenantService
{
    public async Task<List<TenantDto>> GetAllAsync() =>
        await db.Tenants.Select(t => ToDto(t)).ToListAsync();

    public async Task<TenantDto?> GetByIdAsync(Guid id)
    {
        var t = await db.Tenants.FindAsync(id);
        return t is null ? null : ToDto(t);
    }

    public async Task<TenantDto> CreateAsync(CreateTenantDto dto)
    {
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = dto.Name, CreatedAt = DateTime.UtcNow, IsActive = true };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();
        return ToDto(tenant);
    }

    public async Task<TenantDto?> UpdateAsync(Guid id, UpdateTenantDto dto)
    {
        var tenant = await db.Tenants.FindAsync(id);
        if (tenant is null) return null;
        tenant.Name = dto.Name;
        tenant.IsActive = dto.IsActive;
        await db.SaveChangesAsync();
        return ToDto(tenant);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var tenant = await db.Tenants.FindAsync(id);
        if (tenant is null) return false;
        db.Tenants.Remove(tenant);
        await db.SaveChangesAsync();
        return true;
    }

    private static TenantDto ToDto(Tenant t) => new(t.Id, t.Name, t.CreatedAt, t.IsActive);
}
