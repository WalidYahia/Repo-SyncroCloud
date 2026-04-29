using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Data;
using SyncroInfraLayer.Entities;
using SyncroInfraLayer.Enums;
using SyncroInfraLayer.Helpers;

namespace SyncroApplicationLayer.Services;

public class DeviceSensorService(SyncroDbContext db, IMqttService mqtt) : IDeviceSensorService
{
    public async Task<List<DeviceSensorDto>> GetByDeviceAsync(string deviceId) =>
        await db.DeviceSensors.Where(ds => ds.DeviceId == deviceId).Select(ds => ToDto(ds)).ToListAsync();

    public async Task<DeviceSensorDto?> GetByIdAsync(string id)
    {
        var ds = await db.DeviceSensors.FindAsync(id);
        return ds is null ? null : ToDto(ds);
    }

    public async Task<DeviceSensorDto> InstallAsync(CreateDeviceSensorDto dto)
    {
        var ds = new DeviceSensor
        {
            Id               = DeviceSensor.ComputeId(dto.DeviceId, dto.SensorType, dto.UnitType, dto.UnitId, dto.SwitchNo, dto.Address, dto.Port),
            DeviceId         = dto.DeviceId,
            SensorId         = dto.SensorId,
            SwitchNo         = dto.SwitchNo,
            UnitId           = dto.UnitId,
            Address          = dto.Address,
            Port             = dto.Port,
            DisplayName      = dto.DisplayName,
            Url              = dto.Url,
            UnitType         = dto.UnitType,
            SensorType       = dto.SensorType,
            Protocol         = dto.Protocol,
            SyncPeriodicity      = dto.SyncPeriodicity,
            EventChangeSync      = dto.EventChangeSync,
            EventChangeDelta     = dto.EventChangeDelta,
            IsInInchingMode      = dto.IsInInchingMode,
            InchingModeWidthInMs = dto.InchingModeWidthInMs,
            InstalledById        = dto.InstalledById,
            InstalledAt      = DateTime.UtcNow,
            IsActive         = true
        };
        db.DeviceSensors.Add(ds);
        await db.SaveChangesAsync();
        await PublishSensorConfigAsync(ds.DeviceId);
        return ToDto(ds);
    }

    public async Task<DeviceSensorDto?> UpdateAsync(string id, UpdateDeviceSensorDto dto)
    {
        var ds = await db.DeviceSensors.FindAsync(id);
        if (ds is null) return null;
        ds.SwitchNo         = dto.SwitchNo;
        ds.UnitId           = dto.UnitId;
        ds.Address          = dto.Address;
        ds.Port             = dto.Port;
        ds.DisplayName      = dto.DisplayName;
        ds.Url              = dto.Url;
        ds.UnitType         = dto.UnitType;
        ds.SensorType       = dto.SensorType;
        ds.Protocol         = dto.Protocol;
        ds.SyncPeriodicity      = dto.SyncPeriodicity;
        ds.EventChangeSync      = dto.EventChangeSync;
        ds.EventChangeDelta     = dto.EventChangeDelta;
        ds.IsInInchingMode      = dto.IsInInchingMode;
        ds.InchingModeWidthInMs = dto.InchingModeWidthInMs;
        ds.IsActive             = dto.IsActive;
        ds.Notes                = dto.Notes;
        await db.SaveChangesAsync();
        await PublishSensorConfigAsync(ds.DeviceId);
        return ToDto(ds);
    }

    public async Task<bool> UpdateLastReadingAsync(string deviceId, Guid sensorId, string json)
    {
        var ds = await db.DeviceSensors
            .Where(ds => ds.DeviceId == deviceId && ds.SensorId == sensorId && ds.IsActive)
            .OrderByDescending(ds => ds.InstalledAt)
            .FirstOrDefaultAsync();
        if (ds is null) return false;
        db.Entry(ds).Property<string?>("LastReading").CurrentValue = json;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UninstallAsync(string id)
    {
        var ds = await db.DeviceSensors.FindAsync(id);
        if (ds is null) return false;
        var deviceId = ds.DeviceId;
        db.DeviceSensors.Remove(ds);
        await db.SaveChangesAsync();
        await PublishSensorConfigAsync(deviceId);
        return true;
    }

    // DeviceId IS the PK now — no extra lookup needed
    private async Task PublishSensorConfigAsync(string deviceId)
    {
        var sensors = await db.DeviceSensors
            .Where(ds => ds.DeviceId == deviceId)
            .Select(ds => ToDto(ds))
            .ToListAsync();
        await mqtt.PublishAsync(MqttHelper.GetMqttTopic(MqttTopics.CloudSensorConfig, deviceId), sensors, retainFlag: true);
    }

    private static DeviceSensorDto ToDto(DeviceSensor ds) =>
        new(ds.Id, ds.DeviceId, ds.SensorId, ds.SwitchNo, ds.UnitId, ds.Address, ds.Port,
            ds.DisplayName, ds.Url, ds.UnitType, ds.SensorType, ds.Protocol,
            ds.SyncPeriodicity, ds.EventChangeSync, ds.EventChangeDelta,
            ds.IsInInchingMode, ds.InchingModeWidthInMs,
            ds.InstalledAt, ds.IsActive, ds.Notes,
            EF.Property<string?>(ds, "LastReading"));
}
