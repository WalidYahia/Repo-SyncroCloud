using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Data;
using SyncroInfraLayer.Entities;

namespace SyncroApplicationLayer.Services;

public class DeviceSensorService(SyncroDbContext db) : IDeviceSensorService
{
    public async Task<List<DeviceSensorDto>> GetByDeviceAsync(Guid deviceId) =>
        await db.DeviceSensors.Where(ds => ds.DeviceId == deviceId).Select(ds => ToDto(ds)).ToListAsync();

    public async Task<DeviceSensorDto?> GetByIdAsync(Guid deviceId, Guid sensorId)
    {
        var ds = await db.DeviceSensors.FindAsync(deviceId, sensorId);
        return ds is null ? null : ToDto(ds);
    }

    public async Task<DeviceSensorDto> InstallAsync(CreateDeviceSensorDto dto)
    {
        var ds = new DeviceSensor
        {
            DeviceId = dto.DeviceId,
            SensorId = dto.SensorId,
            SwitchNo = dto.SwitchNo,
            UnitId = dto.UnitId,
            Address = dto.Address,
            Port = dto.Port,
            DisplayName = dto.DisplayName,
            Url = dto.Url,
            UnitType = dto.UnitType,
            SensorType = dto.SensorType,
            Protocol = dto.Protocol,
            InstalledById = dto.InstalledById,
            InstalledAt = DateTime.UtcNow,
            IsActive = true
        };
        db.DeviceSensors.Add(ds);
        await db.SaveChangesAsync();
        return ToDto(ds);
    }

    public async Task<DeviceSensorDto?> UpdateAsync(Guid deviceId, Guid sensorId, UpdateDeviceSensorDto dto)
    {
        var ds = await db.DeviceSensors.FindAsync(deviceId, sensorId);
        if (ds is null) return null;
        ds.SwitchNo = dto.SwitchNo;
        ds.UnitId = dto.UnitId;
        ds.Address = dto.Address;
        ds.Port = dto.Port;
        ds.DisplayName = dto.DisplayName;
        ds.Url = dto.Url;
        ds.UnitType = dto.UnitType;
        ds.SensorType = dto.SensorType;
        ds.Protocol = dto.Protocol;
        ds.IsActive = dto.IsActive;
        ds.Notes = dto.Notes;
        await db.SaveChangesAsync();
        return ToDto(ds);
    }

    public async Task<bool> UpdateLastReadingAsync(Guid deviceId, Guid sensorId, string json)
    {
        var ds = await db.DeviceSensors.FindAsync(deviceId, sensorId);
        if (ds is null) return false;
        ds.LastReading = json;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UninstallAsync(Guid deviceId, Guid sensorId)
    {
        var ds = await db.DeviceSensors.FindAsync(deviceId, sensorId);
        if (ds is null) return false;
        db.DeviceSensors.Remove(ds);
        await db.SaveChangesAsync();
        return true;
    }

    private static DeviceSensorDto ToDto(DeviceSensor ds) =>
        new(ds.DeviceId, ds.SensorId, ds.SwitchNo, ds.UnitId, ds.Address, ds.Port,
            ds.DisplayName, ds.Url, ds.UnitType, ds.SensorType, ds.Protocol,
            ds.InstalledAt, ds.IsActive, ds.Notes, ds.LastReading);
}
