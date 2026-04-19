using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Data;
using SyncroInfraLayer.Entities;

namespace SyncroApplicationLayer.Services;

public class DeviceReadingService(SyncroDbContext db) : IDeviceReadingService
{
    public async Task<List<DeviceReadingDto>> GetAsync(Guid deviceId, Guid sensorId, DateTime? from, DateTime? to)
    {
        var query = db.DeviceReadings.Where(r => r.DeviceId == deviceId && r.SensorId == sensorId);
        if (from.HasValue) query = query.Where(r => r.RecordedAt >= from.Value);
        if (to.HasValue)   query = query.Where(r => r.RecordedAt <= to.Value);
        return await query.OrderBy(r => r.RecordedAt).Select(r => ToDto(r)).ToListAsync();
    }

    public async Task<DeviceReadingDto> AddAsync(CreateDeviceReadingDto dto)
    {
        var reading = new DeviceReading
        {
            Id = Guid.NewGuid(),
            DeviceId = dto.DeviceId,
            SensorId = dto.SensorId,
            RecordedAt = dto.RecordedAt,
            ReceivedAt = DateTime.UtcNow,
            Payload = dto.Payload
        };
        db.DeviceReadings.Add(reading);
        await db.SaveChangesAsync();
        return ToDto(reading);
    }

    private static DeviceReadingDto ToDto(DeviceReading r) =>
        new(r.Id, r.DeviceId, r.SensorId, r.RecordedAt, r.ReceivedAt, r.Payload);
}
