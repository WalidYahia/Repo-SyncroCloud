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
        var sensor = new Sensor { SensorId = Guid.NewGuid(), Name = dto.Name, UnitType = dto.UnitType, Type = dto.Type, ConnectionProtocol = dto.ConnectionProtocol };
        db.Sensors.Add(sensor);
        await db.SaveChangesAsync();
        return ToDto(sensor);
    }

    public async Task<SensorDto?> UpdateAsync(Guid id, UpdateSensorDto dto)
    {
        var sensor = await db.Sensors.FindAsync(id);
        if (sensor is null) return null;
        sensor.Name = dto.Name;
        sensor.UnitType = dto.UnitType;
        sensor.Type = dto.Type;
        sensor.ConnectionProtocol = dto.ConnectionProtocol;
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

    private static SensorDto ToDto(Sensor s) => new(s.SensorId, s.Name, s.UnitType, s.Type, s.ConnectionProtocol);
}
