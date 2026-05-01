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
        await db.DeviceSensors
            .Where(ds => ds.DeviceId == deviceId)
            .Select(ds => ToDto(ds, EF.Property<string?>(ds, "LastReading")))
            .ToListAsync();

    public async Task<DeviceSensorDto?> GetByIdAsync(string id)
    {
        var ds = await db.DeviceSensors.FindAsync(id);
        return ds is null ? null : TrackedToDto(ds);
    }

    public async Task<DeviceSensorDto> InstallAsync(CreateDeviceSensorDto dto)
    {
        var ds = new DeviceSensor
        {
            Id               = DeviceSensor.ComputeId(dto.DeviceId, dto.SensorType, dto.UnitId, dto.SwitchNo, dto.Address, dto.Port),
            DeviceId         = dto.DeviceId,
            SensorId         = dto.SensorId,
            SwitchNo         = dto.SwitchNo,
            UnitId           = dto.UnitId,
            Address          = dto.Address,
            Port             = dto.Port,
            DisplayName      = dto.DisplayName,
            Url              = dto.Url,
            SensorType       = dto.SensorType,
            Protocol         = dto.Protocol,
            BaseUrl          = dto.BaseUrl     ?? string.Empty,
            PortNo           = dto.PortNo      ?? string.Empty,
            DataPath         = dto.DataPath    ?? string.Empty,
            InfoPath         = dto.InfoPath    ?? string.Empty,
            InchingPath      = dto.InchingPath ?? string.Empty,
            SyncPeriodicity  = dto.SyncPeriodicity,
            EventChangeSync  = dto.EventChangeSync,
            EventChangeDelta = dto.EventChangeDelta,
            IsInInchingMode  = dto.IsInInchingMode,
            InchingModeWidthInMs = dto.InchingModeWidthInMs,
            InstalledById    = dto.InstalledById,
            InstalledAt      = DateTime.UtcNow,
            IsActive         = true
        };
        db.DeviceSensors.Add(ds);
        await db.SaveChangesAsync();
        await PublishSensorConfigAsync(ds.DeviceId);
        return TrackedToDto(ds);
    }

    public async Task<DeviceSensorDto?> UpdateAsync(string id, UpdateDeviceSensorDto dto)
    {
        var ds = await db.DeviceSensors.FindAsync(id);
        if (ds is null) return null;
        ds.SwitchNo          = dto.SwitchNo;
        ds.UnitId            = dto.UnitId;
        ds.Address           = dto.Address;
        ds.Port              = dto.Port;
        ds.DisplayName       = dto.DisplayName;
        ds.Url               = dto.Url;
        ds.SensorType        = dto.SensorType;
        ds.Protocol          = dto.Protocol;
        ds.BaseUrl           = dto.BaseUrl;
        ds.PortNo            = dto.PortNo;
        ds.DataPath          = dto.DataPath;
        ds.InfoPath          = dto.InfoPath;
        ds.InchingPath       = dto.InchingPath;
        ds.SyncPeriodicity   = dto.SyncPeriodicity;
        ds.EventChangeSync   = dto.EventChangeSync;
        ds.EventChangeDelta  = dto.EventChangeDelta;
        ds.IsInInchingMode   = dto.IsInInchingMode;
        ds.InchingModeWidthInMs = dto.InchingModeWidthInMs;
        ds.IsActive          = dto.IsActive;
        ds.Notes             = dto.Notes;
        await db.SaveChangesAsync();
        await PublishSensorConfigAsync(ds.DeviceId);
        return TrackedToDto(ds);
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

    public async Task SyncFromDeviceAsync(string deviceId, List<DeviceSensorSyncDto> incoming)
    {
        var existing = await db.DeviceSensors
            .Where(ds => ds.DeviceId == deviceId)
            .ToListAsync();

        var existingById = existing.ToDictionary(ds => ds.Id);
        var incomingIds  = new HashSet<string>();

        foreach (var dto in incoming)
        {
            var sensorType = (SensorType)dto.SensorType;
            var switchNo   = (SwitchNo)dto.SwitchNo;
            var id = DeviceSensor.ComputeId(deviceId, sensorType, dto.UnitId, switchNo, dto.Address, dto.Port);
            incomingIds.Add(id);

            if (existingById.TryGetValue(id, out var ds))
            {
                ds.DisplayName       = dto.DisplayName;
                ds.Url               = dto.Url;
                ds.SensorType        = sensorType;
                ds.Protocol          = dto.Protocol;
                ds.DataPath          = dto.DataPath;
                ds.InfoPath          = dto.InfoPath;
                ds.InchingPath       = dto.InchingPath;
                ds.SyncPeriodicity   = dto.SyncPeriodicity;
                ds.EventChangeSync   = dto.EventChangeSync;
                ds.EventChangeDelta  = dto.EventChangeDelta;
                ds.IsInInchingMode   = dto.IsInInchingMode;
                ds.InchingModeWidthInMs = dto.InchingModeWidthInMs;
                ds.IsActive          = dto.IsActive;
                ds.Notes             = dto.Notes;
            }
            else
            {
                db.DeviceSensors.Add(new DeviceSensor
                {
                    Id           = id,
                    DeviceId     = deviceId,
                    SensorId     = dto.SensorId,
                    SwitchNo     = switchNo,
                    UnitId       = dto.UnitId,
                    Address      = dto.Address,
                    Port         = dto.Port,
                    DisplayName  = dto.DisplayName,
                    Url          = dto.Url,
                    SensorType   = sensorType,
                    Protocol     = dto.Protocol,
                    DataPath     = dto.DataPath,
                    InfoPath     = dto.InfoPath,
                    InchingPath  = dto.InchingPath,
                    SyncPeriodicity  = dto.SyncPeriodicity,
                    EventChangeSync  = dto.EventChangeSync,
                    EventChangeDelta = dto.EventChangeDelta,
                    IsInInchingMode  = dto.IsInInchingMode,
                    InchingModeWidthInMs = dto.InchingModeWidthInMs,
                    IsActive    = dto.IsActive,
                    Notes       = dto.Notes,
                    InstalledAt = dto.InstalledAt
                });
            }
        }

        foreach (var ds in existing.Where(ds => !incomingIds.Contains(ds.Id)))
            db.DeviceSensors.Remove(ds);

        await db.SaveChangesAsync();
        await PublishSensorConfigAsync(deviceId);
    }

    private async Task PublishSensorConfigAsync(string deviceId)
    {
        var sensors = await db.DeviceSensors
            .Where(ds => ds.DeviceId == deviceId)
            .Select(ds => ToDto(ds, EF.Property<string?>(ds, "LastReading")))
            .ToListAsync();
        await mqtt.PublishAsync(MqttHelper.GetMqttTopic(MqttTopics.CloudSensorConfig, deviceId), sensors, retainFlag: true);
    }

    // For use inside EF LINQ .Select() — lastReading passed via EF.Property<string?>
    private static DeviceSensorDto ToDto(DeviceSensor ds, string? lastReading) =>
        new(ds.Id, ds.DeviceId, ds.SensorId, ds.SwitchNo, ds.UnitId, ds.Address, ds.Port,
            ds.DisplayName, ds.Url, ds.SensorType, ds.Protocol,
            ds.BaseUrl, ds.PortNo, ds.DataPath, ds.InfoPath, ds.InchingPath,
            ds.SyncPeriodicity, ds.EventChangeSync, ds.EventChangeDelta,
            ds.IsInInchingMode, ds.InchingModeWidthInMs,
            ds.InstalledAt, ds.IsActive, ds.Notes,
            lastReading);

    // For use with tracked entity instances outside LINQ
    private DeviceSensorDto TrackedToDto(DeviceSensor ds) =>
        ToDto(ds, db.Entry(ds).Property<string?>("LastReading").CurrentValue);
}
