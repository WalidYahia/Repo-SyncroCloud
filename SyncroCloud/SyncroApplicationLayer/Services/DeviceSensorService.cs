using System.Text.Json;
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
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    // ── Queries ───────────────────────────────────────────────

    public async Task<List<DeviceSensorDto>> GetByDeviceAsync(string deviceId)
    {
        var config = await db.DeviceConfigs
            .FirstOrDefaultAsync(c => c.DeviceId == deviceId && c.ConfigType == ConfigType.Sensor);
        if (config is null) return [];

        var sensors = Deserialize(config.Config);
        var sensorIds = sensors.Select(s => s.Id).ToList();
        var latestReadings = await db.DeviceLatestReadings
            .Where(r => sensorIds.Contains(r.DeviceSensorId))
            .ToDictionaryAsync(r => r.DeviceSensorId);

        return sensors.Select(s =>
        {
            var latest = latestReadings.GetValueOrDefault(s.Id);
            return ToDto(s, ExtractValue(latest?.Payload), latest?.ReadingTime);
        }).ToList();
    }

    public async Task<DeviceSensorDto?> GetByIdAsync(string id)
    {
        var (_, sensor) = await FindConfigAndSensorAsync(id);
        if (sensor is null) return null;

        var latest = await db.DeviceLatestReadings.FindAsync(id);
        return ToDto(sensor, ExtractValue(latest?.Payload), latest?.ReadingTime);
    }

    // ── Writes (cloud-originated) ─────────────────────────────

    public async Task<DeviceSensorDto> InstallAsync(CreateDeviceSensorDto dto)
    {
        var sensor = await db.Sensors.FindAsync(dto.SensorId)
            ?? throw new KeyNotFoundException($"Sensor template '{dto.SensorId}' not found");

        var newSensor = new DeviceSensorSyncDto(
            SensorIdHelper.ComputeId(dto.DeviceId, dto.SensorType, dto.UnitId, dto.SwitchNo, dto.Address, dto.Port),
            dto.DeviceId, dto.SensorId,
            (int)dto.SwitchNo, dto.UnitId, dto.Address, dto.Port,
            dto.DisplayName, dto.Url,
            (int)dto.SensorType, dto.Protocol,
            dto.DataPath ?? string.Empty, dto.InfoPath ?? string.Empty, dto.InchingPath ?? string.Empty,
            dto.SyncPeriodicity, sensor.EventChangeSync, dto.EventChangeDelta, sensor.OnlySaveRecordOnChange,
            dto.IsInInchingMode, dto.IsInInchingMode ? dto.InchingModeWidthInMs : 0,
            DateTime.UtcNow, true, null);

        var config = await GetOrCreateConfigAsync(dto.DeviceId);
        var sensors = Deserialize(config.Config);
        sensors.Add(newSensor);
        await SaveAndPublishAsync(config, sensors);

        return ToDto(newSensor, null, null);
    }

    public async Task<DeviceSensorDto?> UpdateAsync(string id, UpdateDeviceSensorDto dto)
    {
        var (config, existing) = await FindConfigAndSensorAsync(id);
        if (config is null || existing is null) return null;

        var sensors = Deserialize(config.Config);
        var idx = sensors.FindIndex(s => s.Id == id);
        if (idx < 0) return null;

        var updated = existing with
        {
            SwitchNo         = (int)dto.SwitchNo,
            UnitId           = dto.UnitId,
            Address          = dto.Address,
            Port             = dto.Port,
            DisplayName      = dto.DisplayName,
            Url              = dto.Url,
            SensorType       = (int)dto.SensorType,
            Protocol         = dto.Protocol,
            DataPath         = dto.DataPath,
            InfoPath         = dto.InfoPath,
            InchingPath      = dto.InchingPath,
            SyncPeriodicity  = dto.SyncPeriodicity,
            EventChangeSync       = dto.EventChangeSync,
            EventChangeDelta      = dto.EventChangeDelta,
            OnlySaveRecordOnChange = dto.OnlySaveRecordOnChange,
            IsInInchingMode       = dto.IsInInchingMode,
            InchingModeWidthInMs = dto.InchingModeWidthInMs,
            IsActive         = dto.IsActive,
            Notes            = dto.Notes
        };

        sensors[idx] = updated;
        await SaveAndPublishAsync(config, sensors);

        var latest = await db.DeviceLatestReadings.FindAsync(id);
        return ToDto(updated, ExtractValue(latest?.Payload), latest?.ReadingTime);
    }

    public async Task<bool> UpdateDisplayNameAsync(string id, string name)
    {
        var (config, existing) = await FindConfigAndSensorAsync(id);
        if (config is null || existing is null) return false;

        var sensors = Deserialize(config.Config);
        var idx = sensors.FindIndex(s => s.Id == id);
        if (idx < 0) return false;

        sensors[idx] = existing with { DisplayName = name };
        await SaveAndPublishAsync(config, sensors);
        return true;
    }

    public async Task<bool> UpdateInchingAsync(string id, bool enabled, int widthMs)
    {
        var (config, existing) = await FindConfigAndSensorAsync(id);
        if (config is null || existing is null) return false;

        var sensors = Deserialize(config.Config);
        var idx = sensors.FindIndex(s => s.Id == id);
        if (idx < 0) return false;

        sensors[idx] = existing with
        {
            IsInInchingMode      = enabled,
            InchingModeWidthInMs = enabled ? widthMs : 0
        };
        await SaveAndPublishAsync(config, sensors);
        return true;
    }

    public async Task<bool> UninstallAsync(string id)
    {
        var (config, existing) = await FindConfigAndSensorAsync(id);
        if (config is null || existing is null) return false;

        var sensors = Deserialize(config.Config);
        sensors.RemoveAll(s => s.Id == id);
        await SaveAndPublishAsync(config, sensors);
        return true;
    }

    // ── Hub sync (hub-originated, no publish back) ────────────

    public async Task SyncFromDeviceAsync(string deviceId, SensorConfigEnvelope envelope)
    {
        var config = await db.DeviceConfigs
            .FirstOrDefaultAsync(c => c.DeviceId == deviceId && c.ConfigType == ConfigType.Sensor);

        // Same version — already in sync
        if (config is not null && config.ConfigVersion == envelope.ConfigVersion)
            return;

        // Different version but cloud is more recent — keep cloud's copy
        if (config is not null && envelope.UpdateTime <= config.UpdateTime)
            return;

        var updateTime = DateTime.SpecifyKind(envelope.UpdateTime, DateTimeKind.Utc);

        if (config is null)
        {
            db.DeviceConfigs.Add(new DeviceConfig
            {
                DeviceId      = deviceId,
                ConfigType    = ConfigType.Sensor,
                ConfigVersion = envelope.ConfigVersion,
                UpdateTime    = updateTime,
                Config        = JsonSerializer.Serialize(envelope.Sensors, _json),
                UpdatedFrom   = ConfigSource.Local
            });
        }
        else
        {
            config.Config        = JsonSerializer.Serialize(envelope.Sensors, _json);
            config.ConfigVersion = envelope.ConfigVersion;
            config.UpdateTime    = updateTime;
            config.UpdatedFrom   = ConfigSource.Local;
        }

        await db.SaveChangesAsync();
        // No publish — hub is already up to date
    }

    // ── Private helpers ───────────────────────────────────────

    private async Task<DeviceConfig> GetOrCreateConfigAsync(string deviceId)
    {
        var config = await db.DeviceConfigs
            .FirstOrDefaultAsync(c => c.DeviceId == deviceId && c.ConfigType == ConfigType.Sensor);

        if (config is null)
        {
            config = new DeviceConfig
            {
                DeviceId    = deviceId,
                ConfigType  = ConfigType.Sensor,
                UpdatedFrom = ConfigSource.Local
            };
            db.DeviceConfigs.Add(config);
        }

        return config;
    }

    private async Task<(DeviceConfig? config, DeviceSensorSyncDto? sensor)> FindConfigAndSensorAsync(string sensorId)
    {
        var configs = await db.DeviceConfigs
            .Where(c => c.ConfigType == ConfigType.Sensor)
            .ToListAsync();

        foreach (var config in configs)
        {
            if (!sensorId.StartsWith(config.DeviceId + "_")) continue;
            var sensor = Deserialize(config.Config).FirstOrDefault(s => s.Id == sensorId);
            if (sensor is not null) return (config, sensor);
        }

        return (null, null);
    }

    private async Task SaveAndPublishAsync(DeviceConfig config, List<DeviceSensorSyncDto> sensors)
    {
        config.Config        = JsonSerializer.Serialize(sensors, _json);
        config.ConfigVersion = Guid.NewGuid();
        config.UpdateTime    = DateTime.UtcNow;
        config.UpdatedFrom   = ConfigSource.Local;

        await db.SaveChangesAsync();

        var envelope = new SensorConfigEnvelope(config.ConfigVersion, config.UpdateTime, sensors);
        await mqtt.PublishAsync(
            MqttHelper.GetMqttTopic(MqttTopics.CloudSensorConfig, config.DeviceId),
            envelope,
            retainFlag: true);
    }

    private static List<DeviceSensorSyncDto> Deserialize(string json) =>
        JsonSerializer.Deserialize<List<DeviceSensorSyncDto>>(json, _json) ?? [];

    private static string? ExtractValue(string? payload)
    {
        if (payload is null) return null;
        try
        {
            using var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("value", out var v))
                return v.ToString();
        }
        catch { }
        return null;
    }

    private static DeviceSensorDto ToDto(DeviceSensorSyncDto s, string? lastReading, DateTime? lastSeen) =>
        new(s.Id, s.DeviceId, s.SensorId,
            (SwitchNo)s.SwitchNo, s.UnitId, s.Address, s.Port,
            s.DisplayName, s.Url,
            (SensorType)s.SensorType, s.Protocol,
            s.DataPath, s.InfoPath, s.InchingPath,
            s.SyncPeriodicity, s.EventChangeSync, s.EventChangeDelta, s.OnlySaveRecordOnChange,
            s.IsInInchingMode, s.InchingModeWidthInMs,
            s.InstalledAt, s.IsActive, s.Notes,
            lastReading, lastSeen);
}
