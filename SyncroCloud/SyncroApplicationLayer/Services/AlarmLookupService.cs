using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Data;
using SyncroInfraLayer.Entities;
using SyncroInfraLayer.Enums;

namespace SyncroApplicationLayer.Services;

public class AlarmLookupService(SyncroDbContext db) : IAlarmLookupService
{
    public async Task<List<AlarmLookupDto>> GetAllAsync() =>
        await db.AlarmLookups.Select(a => ToDto(a)).ToListAsync();

    public async Task<List<AlarmLookupDto>> GetBySensorTypeAsync(SensorType type) =>
        await db.AlarmLookups.Where(a => a.SensorType == type).Select(a => ToDto(a)).ToListAsync();

    public async Task<AlarmLookupDto?> GetByIdAsync(int id)
    {
        var a = await db.AlarmLookups.FindAsync(id);
        return a is null ? null : ToDto(a);
    }

    public async Task<AlarmLookupDto> CreateAsync(CreateAlarmLookupDto dto)
    {
        var alarm = new AlarmLookup
        {
            SensorType = dto.SensorType,
            MeasurementKey = dto.MeasurementKey,
            Condition = dto.Condition,
            DefaultThreshold = dto.DefaultThreshold,
            DefaultThresholdMax = dto.DefaultThresholdMax,
            Severity = dto.Severity,
            Description = dto.Description,
            IsActive = true
        };
        db.AlarmLookups.Add(alarm);
        await db.SaveChangesAsync();
        return ToDto(alarm);
    }

    public async Task<AlarmLookupDto?> UpdateAsync(int id, UpdateAlarmLookupDto dto)
    {
        var alarm = await db.AlarmLookups.FindAsync(id);
        if (alarm is null) return null;
        alarm.Condition = dto.Condition;
        alarm.DefaultThreshold = dto.DefaultThreshold;
        alarm.DefaultThresholdMax = dto.DefaultThresholdMax;
        alarm.Severity = dto.Severity;
        alarm.Description = dto.Description;
        alarm.IsActive = dto.IsActive;
        await db.SaveChangesAsync();
        return ToDto(alarm);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var alarm = await db.AlarmLookups.FindAsync(id);
        if (alarm is null) return false;
        db.AlarmLookups.Remove(alarm);
        await db.SaveChangesAsync();
        return true;
    }

    private static AlarmLookupDto ToDto(AlarmLookup a) =>
        new(a.Id, a.SensorType, a.MeasurementKey, a.Condition, a.DefaultThreshold, a.DefaultThresholdMax, a.Severity, a.Description, a.IsActive);
}
