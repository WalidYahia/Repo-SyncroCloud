using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Data;
using SyncroInfraLayer.Entities;
using SyncroInfraLayer.Enums;
using SyncroInfraLayer.Helpers;

namespace SyncroApplicationLayer.Services;

public class DeviceScenarioService(SyncroDbContext db, IMqttService mqtt) : IDeviceScenarioService
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
                Id        = scenarioId,
                DeviceId  = dto.DeviceId,
                Payload   = dto.Payload,
                UpdatedAt = DateTime.UtcNow
            };
            db.DeviceScenarios.Add(existing);
        }
        else
        {
            existing.Payload   = dto.Payload;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        await PublishScenariosAsync(existing.DeviceId);
        return ToDto(existing);
    }

    public async Task<bool> DeleteAsync(Guid deviceId, Guid scenarioId)
    {
        var s = await db.DeviceScenarios.FindAsync(scenarioId);
        if (s is null || s.DeviceId != deviceId) return false;
        db.DeviceScenarios.Remove(s);
        await db.SaveChangesAsync();
        await PublishScenariosAsync(deviceId);
        return true;
    }

    public async Task<int> DeleteAllByDeviceAsync(Guid deviceId)
    {
        var count = await db.DeviceScenarios
            .Where(s => s.DeviceId == deviceId)
            .ExecuteDeleteAsync();

        if (count > 0)
            await PublishScenariosAsync(deviceId);

        return count;
    }

    private async Task PublishScenariosAsync(Guid devicePk)
    {
        var deviceId = await db.Devices
            .Where(d => d.Id == devicePk)
            .Select(d => d.DeviceId)
            .FirstOrDefaultAsync();
        if (deviceId is null) return;

        var scenarios = await db.DeviceScenarios
            .Where(s => s.DeviceId == devicePk)
            .Select(s => ToDto(s))
            .ToListAsync();

        await mqtt.PublishAsync(MqttHelper.GetMqttTopic(MqttTopics.CloudUserScenario, deviceId), scenarios, retainFlag: true);
    }

    private static DeviceScenarioDto ToDto(DeviceScenario s) =>
        new(s.Id, s.DeviceId, s.Payload, s.UpdatedAt);
}
