using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Data;
using SyncroInfraLayer.Entities;
using SyncroInfraLayer.Enums;

namespace SyncroApplicationLayer.Services;

public class DeviceService(SyncroDbContext db, IUserService userService, IDeviceSensorService sensorService) : IDeviceService
{
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };


    public async Task<List<DeviceDto>> GetAllAsync() =>
        await db.Devices.Select(d => ToDto(d)).ToListAsync();

    public async Task<List<DeviceDto>> GetByTenantAsync(Guid tenantId) =>
        await db.Devices.Where(d => d.TenantId == tenantId).Select(d => ToDto(d)).ToListAsync();

    public async Task<List<DeviceDto>> GetByUserAsync(Guid userId) =>
        await db.DeviceUsers
            .Where(du => du.UserId == userId)
            .Select(du => du.Device)
            .Select(d => ToDto(d))
            .ToListAsync();

    public async Task<DeviceDto?> GetByIdAsync(string id)
    {
        var d = await db.Devices.FindAsync(id);
        return d is null ? null : ToDto(d);
    }

    public async Task<(bool Success, DeviceDto? Device, string? Error)> CreateAsync(CreateDeviceDto dto, Guid createdByUserId)
    {
        if (await db.Devices.AnyAsync(d => d.DeviceId == dto.DeviceId))
            return (false, null, $"Device ID '{dto.DeviceId}' is already registered.");

        if (await db.Devices.AnyAsync(d => d.SerialNumber == dto.SerialNumber))
            return (false, null, $"Serial number '{dto.SerialNumber}' is already registered.");

        var city = await db.Cities.FindAsync(dto.CityId);
        if (city is null)
            return (false, null, $"City with ID {dto.CityId} was not found.");

        var device = new Device
        {
            DeviceId      = dto.DeviceId,
            TenantId      = dto.TenantId,
            CreatedByUser = createdByUserId,
            CityId        = dto.CityId,
            Name          = dto.Name,
            SerialNumber  = dto.SerialNumber,
            Longitude     = city.Longitude,
            Latitude      = city.Latitude,
            Type          = dto.Type,
            Status        = DeviceStatus.Offline,
            RegisteredAt  = DateTime.UtcNow,            
        };

        await using (var tx = await db.Database.BeginTransactionAsync())
        {
            db.Devices.Add(device);
            await db.SaveChangesAsync(); // commit device before bridge rows (resolves EF FK ambiguity)

            var adminIds = await userService.GetTenantAdminIdsAsync(dto.TenantId);
            var linkedUserIds = adminIds.Append(createdByUserId).Distinct();

            foreach (var userId in linkedUserIds)
                db.DeviceUsers.Add(new DeviceUser { DeviceId = device.DeviceId, UserId = userId, LinkedAt = DateTime.UtcNow });

            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }

        return (true, ToDto(device), null);
    }

    public async Task<DeviceDto?> UpdateAsync(string id, UpdateDeviceDto dto)
    {
        var device = await db.Devices.FindAsync(id);
        if (device is null) return null;
        device.Name      = dto.Name;
        device.CityId    = dto.CityId;
        device.Longitude = dto.Longitude;
        device.Latitude  = dto.Latitude;
        device.Status    = dto.Status;
        await db.SaveChangesAsync();
        return ToDto(device);
    }

    public async Task<bool> UpdateStatusAsync(string id, DeviceStatus status)
    {
        var device = await db.Devices.FindAsync(id);
        if (device is null) return false;
        device.Status    = status;
        device.LastSeenAt = status == DeviceStatus.Online ? DateTime.UtcNow : device.LastSeenAt;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateLastSeenAsync(string id, DateTime lastSeenAt)
    {
        var device = await db.Devices.FindAsync(id);
        if (device is null) return false;
        device.LastSeenAt = lastSeenAt;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var device = await db.Devices.FindAsync(id);
        if (device is null) return false;
        db.Devices.Remove(device);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<DeviceUserDto>> GetDeviceUsersAsync(string deviceId)
    {
        var links = await db.DeviceUsers.Where(du => du.DeviceId == deviceId).ToListAsync();
        return links.Select(ToDto).ToList();
    }

    public async Task<DeviceUserDto?> GetUserLinkAsync(string deviceId, Guid userId)
    {
        var du = await db.DeviceUsers.FindAsync(deviceId, userId);
        return du is null ? null : ToDto(du);
    }

    public async Task<DeviceUserDto> AssignUserAsync(string deviceId, Guid userId)
    {
        var existing = await db.DeviceUsers.FindAsync(deviceId, userId);
        if (existing is not null) return ToDto(existing);

        // Grant Watch access to every installed sensor by default.
        var sensors = await sensorService.GetByDeviceAsync(deviceId);
        var permissions = sensors.Select(s => new SensorPermissionDto(s.Id, SensorAccessLevel.Watch)).ToList();

        var du = new DeviceUser
        {
            DeviceId          = deviceId,
            UserId            = userId,
            LinkedAt          = DateTime.UtcNow,
            SensorPermissions = JsonSerializer.Serialize(permissions, _json)
        };
        db.DeviceUsers.Add(du);
        await db.SaveChangesAsync();
        return ToDto(du);
    }

    public async Task<bool> RemoveUserAsync(string deviceId, Guid userId)
    {
        var existing = await db.DeviceUsers.FindAsync(deviceId, userId);
        if (existing is null) return false;
        db.DeviceUsers.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<DeviceUserDto?> UpdateSensorPermissionsAsync(string deviceId, Guid userId, List<SensorPermissionDto> permissions)
    {
        var du = await db.DeviceUsers.FindAsync(deviceId, userId);
        if (du is null) return null;

        du.SensorPermissions = JsonSerializer.Serialize(permissions, _json);
        await db.SaveChangesAsync();
        return ToDto(du);
    }

    private static DeviceDto ToDto(Device d) =>
        new(d.DeviceId, d.TenantId, d.CityId, d.Name, d.SerialNumber, d.Longitude, d.Latitude, d.Type, d.Status, d.RegisteredAt, d.LastSeenAt);

    private static DeviceUserDto ToDto(DeviceUser du) =>
        new(du.DeviceId, du.UserId, du.LinkedAt,
            JsonSerializer.Deserialize<List<SensorPermissionDto>>(du.SensorPermissions, _json) ?? []);
}
