using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Data;
using SyncroInfraLayer.Entities;

namespace SyncroApplicationLayer.Services;

public class DeviceActionLogService(SyncroDbContext db) : IDeviceActionLogService
{
    public async Task LogAsync(CreateDeviceActionLogDto dto)
    {
        db.DeviceActionLogs.Add(new DeviceActionLog
        {
            Id                = Guid.NewGuid(),
            DeviceId          = dto.DeviceId,
            InstalledSensorId = dto.InstalledSensorId,
            Action            = dto.Action,
            Source            = dto.Source,
            Result            = dto.Result,
            Notes             = dto.Notes,
            TriggeredByUserId = dto.TriggeredByUserId,
            Timestamp         = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    public async Task<List<DeviceActionLogDto>> GetByDeviceAsync(string deviceId, int limit = 100) =>
        await db.DeviceActionLogs
            .Where(l => l.DeviceId == deviceId)
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .Select(l => ToDto(l))
            .ToListAsync();

    public async Task<List<DeviceActionLogDto>> GetBySensorAsync(string installedSensorId, int limit = 100) =>
        await db.DeviceActionLogs
            .Where(l => l.InstalledSensorId == installedSensorId)
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .Select(l => ToDto(l))
            .ToListAsync();

    private static DeviceActionLogDto ToDto(DeviceActionLog l) =>
        new(l.Id, l.DeviceId, l.InstalledSensorId, l.Action, l.Source,
            l.TriggeredByUserId, l.Result, l.Notes, l.Timestamp);
}
