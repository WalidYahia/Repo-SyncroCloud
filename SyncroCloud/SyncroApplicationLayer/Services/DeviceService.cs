using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Data;
using SyncroInfraLayer.Entities;
using SyncroInfraLayer.Enums;

namespace SyncroApplicationLayer.Services;

public class DeviceService(SyncroDbContext db) : IDeviceService
{
    public async Task<List<DeviceDto>> GetByTenantAsync(Guid tenantId) =>
        await db.Devices.Where(d => d.TenantId == tenantId).Select(d => ToDto(d)).ToListAsync();

    public async Task<DeviceDto?> GetByIdAsync(Guid id)
    {
        var d = await db.Devices.FindAsync(id);
        return d is null ? null : ToDto(d);
    }

    public async Task<DeviceDto> CreateAsync(CreateDeviceDto dto)
    {
        var device = new Device
        {
            Id = Guid.NewGuid(),
            DeviceId = dto.DeviceId,
            TenantId = dto.TenantId,
            CityId = dto.CityId,
            Name = dto.Name,
            SerialNumber = dto.SerialNumber,
            Longitude = dto.Longitude,
            Latitude = dto.Latitude,
            Type = dto.Type,
            Status = DeviceStatus.Offline,
            RegisteredAt = DateTime.UtcNow
        };
        db.Devices.Add(device);
        await db.SaveChangesAsync();
        return ToDto(device);
    }

    public async Task<DeviceDto?> UpdateAsync(Guid id, UpdateDeviceDto dto)
    {
        var device = await db.Devices.FindAsync(id);
        if (device is null) return null;
        device.Name = dto.Name;
        device.CityId = dto.CityId;
        device.Longitude = dto.Longitude;
        device.Latitude = dto.Latitude;
        device.Status = dto.Status;
        await db.SaveChangesAsync();
        return ToDto(device);
    }

    public async Task<bool> UpdateStatusAsync(Guid id, DeviceStatus status)
    {
        var device = await db.Devices.FindAsync(id);
        if (device is null) return false;
        device.Status = status;
        device.LastSeenAt = status == DeviceStatus.Online ? DateTime.UtcNow : device.LastSeenAt;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var device = await db.Devices.FindAsync(id);
        if (device is null) return false;
        db.Devices.Remove(device);
        await db.SaveChangesAsync();
        return true;
    }

    private static DeviceDto ToDto(Device d) =>
        new(d.Id, d.DeviceId, d.TenantId, d.CityId, d.Name, d.SerialNumber, d.Longitude, d.Latitude, d.Type, d.Status, d.RegisteredAt, d.LastSeenAt);
}
