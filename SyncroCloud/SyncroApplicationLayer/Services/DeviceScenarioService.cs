using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Data;
using SyncroInfraLayer.Entities;

namespace SyncroApplicationLayer.Services;

public class DeviceScenarioService(SyncroDbContext db) : IDeviceScenarioService
{
    public async Task<List<DeviceScenarioDto>> GetByDeviceAsync(Guid deviceId) =>
        await db.DeviceScenarios
            .Where(s => s.DeviceId == deviceId)
            .Select(s => ToDto(s))
            .ToListAsync();

    public async Task<DeviceScenarioDto?> GetByIdAsync(Guid deviceId, Guid scenarioId)
    {
        var s = await db.DeviceScenarios.FindAsync(scenarioId);
        return s is null || s.DeviceId != deviceId ? null : ToDto(s);
    }

    public async Task<DeviceScenarioDto> UpsertAsync(Guid scenarioId, UpsertDeviceScenarioDto dto)
    {
        var existing = await db.DeviceScenarios.FindAsync(scenarioId);
        if (existing is null)
        {
            existing = new DeviceScenario
            {
                Id = scenarioId,
                DeviceId = dto.DeviceId,
                Payload = dto.Payload,
                UpdatedAt = DateTime.UtcNow
            };
            db.DeviceScenarios.Add(existing);
        }
        else
        {
            existing.Payload = dto.Payload;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        return ToDto(existing);
    }

    public async Task<bool> DeleteAsync(Guid deviceId, Guid scenarioId)
    {
        var s = await db.DeviceScenarios.FindAsync(scenarioId);
        if (s is null || s.DeviceId != deviceId) return false;
        db.DeviceScenarios.Remove(s);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<int> DeleteAllByDeviceAsync(Guid deviceId)
    {
        return await db.DeviceScenarios
            .Where(s => s.DeviceId == deviceId)
            .ExecuteDeleteAsync();
    }

    private static DeviceScenarioDto ToDto(DeviceScenario s) =>
        new(s.Id, s.DeviceId, s.Payload, s.UpdatedAt);
}
