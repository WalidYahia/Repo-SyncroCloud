using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Data;
using SyncroInfraLayer.Entities;

namespace SyncroApplicationLayer.Services;

public class DeviceReadingService(SyncroDbContext db) : IDeviceReadingService
{
    public async Task<List<DeviceReadingDto>> GetAsync(string deviceSensorId, DateTime? from, DateTime? to)
    {
        var query = db.DeviceReadings.Where(r => r.DeviceSensorId == deviceSensorId);
        if (from.HasValue) query = query.Where(r => r.ReadingTime >= from.Value);
        if (to.HasValue)   query = query.Where(r => r.ReadingTime <= to.Value);
        return await query.OrderBy(r => r.ReadingTime).Select(r => ToDto(r)).ToListAsync();
    }

    public async Task<DeviceReadingDto> AddAsync(CreateDeviceReadingDto dto)
    {
        var readingTime = ParseTimestamp(dto.Payload, "readingTime");

        var existing = await db.DeviceReadings.FirstOrDefaultAsync(
            r => r.DeviceSensorId == dto.DeviceSensorId && r.ReadingTime == readingTime);
        if (existing is not null)
            return ToDto(existing);

        var publishedAt = ParseTimestamp(dto.Payload, "publishedAt");
        var writeTime    = DateTime.UtcNow;

        var reading = new DeviceReading
        {
            Id             = Guid.NewGuid(),
            DeviceSensorId = dto.DeviceSensorId,
            DeviceId       = dto.DeviceId,
            ReadingTime    = readingTime,
            PublishedAt    = publishedAt,
            WriteTime      = writeTime,
            Payload        = dto.Payload
        };
        db.DeviceReadings.Add(reading);

        await UpsertLatestAsync(dto, readingTime, publishedAt, writeTime);

        await db.SaveChangesAsync();
        return ToDto(reading);
    }

    private async Task UpsertLatestAsync(CreateDeviceReadingDto dto, DateTime readingTime, DateTime publishedAt, DateTime writeTime)
    {
        var latest = await db.DeviceLatestReadings.FindAsync(dto.DeviceSensorId);
        if (latest is not null && latest.ReadingTime >= readingTime)
            return; // already have a newer (or same) reading

        if (latest is null)
        {
            latest = new DeviceLatestReading { DeviceSensorId = dto.DeviceSensorId };
            db.DeviceLatestReadings.Add(latest);
        }

        latest.DeviceId    = dto.DeviceId;
        latest.ReadingTime = readingTime;
        latest.PublishedAt = publishedAt;
        latest.WriteTime   = writeTime;
        latest.Payload     = dto.Payload;
    }

    private static DateTime ParseTimestamp(string payload, string propertyName)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty(propertyName, out var prop))
                return DateTime.SpecifyKind(prop.GetDateTime(), DateTimeKind.Utc);
        }
        catch { }
        return DateTime.UtcNow;
    }

    private static DeviceReadingDto ToDto(DeviceReading r) =>
        new(r.Id, r.DeviceId, r.DeviceSensorId, r.ReadingTime, r.PublishedAt, r.WriteTime, r.Payload);
}
