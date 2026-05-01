using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Data;
using SyncroInfraLayer.Entities;

namespace SyncroApplicationLayer.Services;

public class SensorService(SyncroDbContext db) : ISensorService
{
    public async Task<List<SensorDto>> GetAllAsync() =>
        await db.Sensors.Select(s => ToDto(s)).ToListAsync();

    public async Task<SensorDto?> GetByIdAsync(Guid id)
    {
        var s = await db.Sensors.FindAsync(id);
        return s is null ? null : ToDto(s);
    }

    public async Task<SensorDto> CreateAsync(CreateSensorDto dto)
    {
        var sensor = new Sensor
        {
            SensorId           = Guid.NewGuid(),
            Name               = dto.Name,
            Type               = dto.Type,
            ConnectionProtocol = dto.ConnectionProtocol,
            BaseUrl            = dto.BaseUrl,
            PortNo             = dto.PortNo,
            DataPath           = dto.DataPath,
            InfoPath           = dto.InfoPath,
            InchingPath        = dto.InchingPath,
            SyncPeriodicity    = dto.SyncPeriodicity,
            EventChangeSync    = dto.EventChangeSync,
            EventChangeDelta   = dto.EventChangeDelta
        };
        db.Sensors.Add(sensor);
        await db.SaveChangesAsync();
        return ToDto(sensor);
    }

    public async Task<SensorDto?> UpdateAsync(Guid id, UpdateSensorDto dto)
    {
        var sensor = await db.Sensors.FindAsync(id);
        if (sensor is null) return null;
        sensor.Name               = dto.Name;
        sensor.Type               = dto.Type;
        sensor.ConnectionProtocol = dto.ConnectionProtocol;
        sensor.BaseUrl            = dto.BaseUrl;
        sensor.PortNo             = dto.PortNo;
        sensor.DataPath           = dto.DataPath;
        sensor.InfoPath           = dto.InfoPath;
        sensor.InchingPath        = dto.InchingPath;
        sensor.SyncPeriodicity    = dto.SyncPeriodicity;
        sensor.EventChangeSync    = dto.EventChangeSync;
        sensor.EventChangeDelta   = dto.EventChangeDelta;
        await db.SaveChangesAsync();
        return ToDto(sensor);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var sensor = await db.Sensors.FindAsync(id);
        if (sensor is null) return false;
        db.Sensors.Remove(sensor);
        await db.SaveChangesAsync();
        return true;
    }

    private static SensorDto ToDto(Sensor s) =>
        new(s.SensorId, s.Name,
            s.Type, s.ConnectionProtocol,
            s.BaseUrl, s.PortNo,
            s.DataPath, s.InfoPath, s.InchingPath,
            s.SyncPeriodicity, s.EventChangeSync, s.EventChangeDelta);
}
