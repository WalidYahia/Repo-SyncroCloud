using System.Text.Json;
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
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public async Task<List<MqttUserScenarioDto>> GetByDeviceAsync(string deviceId)
    {
        var config = await GetConfigAsync(deviceId, ConfigSource.Cloud);
        return config is null ? [] : Deserialize(config.Config);
    }

    public async Task<MqttUserScenarioDto?> GetByIdAsync(string deviceId, string scenarioId)
    {
        var scenarios = await GetByDeviceAsync(deviceId);
        return scenarios.FirstOrDefault(s => s.Id == scenarioId);
    }

    public async Task<MqttUserScenarioDto> CreateAsync(string deviceId, MqttUserScenarioDto scenario)
    {
        var config = await GetOrCreateConfigAsync(deviceId, ConfigSource.Cloud);
        var scenarios = Deserialize(config.Config);

        var newScenario = string.IsNullOrWhiteSpace(scenario.Id)
            ? scenario with { Id = Guid.NewGuid().ToString() }
            : scenario;

        scenarios.RemoveAll(s => s.Id == newScenario.Id);
        scenarios.Add(newScenario);

        await SaveAndPublishAsync(config, scenarios);
        return newScenario;
    }

    public async Task<MqttUserScenarioDto?> UpdateAsync(string deviceId, string scenarioId, MqttUserScenarioDto scenario)
    {
        var config = await GetConfigAsync(deviceId, ConfigSource.Cloud);
        if (config is null) return null;

        var scenarios = Deserialize(config.Config);
        var idx = scenarios.FindIndex(s => s.Id == scenarioId);
        if (idx < 0) return null;

        var updated = scenario with { Id = scenarioId };
        scenarios[idx] = updated;

        await SaveAndPublishAsync(config, scenarios);
        return updated;
    }

    public async Task<bool> DeleteAsync(string deviceId, string scenarioId)
    {
        var config = await GetConfigAsync(deviceId, ConfigSource.Cloud);
        if (config is null) return false;

        var scenarios = Deserialize(config.Config);
        if (scenarios.RemoveAll(s => s.Id == scenarioId) == 0) return false;

        await SaveAndPublishAsync(config, scenarios);
        return true;
    }

    // ── Hub sync (device-originated, no publish back) ─────────

    public async Task SyncFromDeviceAsync(string deviceId, ScenarioConfigEnvelope envelope)
    {
        var config = await GetOrCreateConfigAsync(deviceId, ConfigSource.Device);

        config.Config        = JsonSerializer.Serialize(envelope.Scenarios, _json);
        config.ConfigVersion = envelope.ConfigVersion;
        config.UpdateTime    = DateTime.SpecifyKind(envelope.UpdateTime, DateTimeKind.Utc);
        config.UpdatedFrom   = ConfigSource.Device;

        await db.SaveChangesAsync();
        // No publish — hub already knows its own config
    }

    private Task<DeviceConfig?> GetConfigAsync(string deviceId, ConfigSource source) =>
        db.DeviceConfigs.FirstOrDefaultAsync(c => c.DeviceId == deviceId && c.ConfigType == ConfigType.Scenario && c.UpdatedFrom == source);

    private async Task<DeviceConfig> GetOrCreateConfigAsync(string deviceId, ConfigSource source)
    {
        var config = await GetConfigAsync(deviceId, source);
        if (config is null)
        {
            config = new DeviceConfig
            {
                DeviceId    = deviceId,
                ConfigType  = ConfigType.Scenario,
                UpdatedFrom = source
            };
            db.DeviceConfigs.Add(config);
        }

        return config;
    }

    private async Task SaveAndPublishAsync(DeviceConfig config, List<MqttUserScenarioDto> scenarios)
    {
        config.Config        = JsonSerializer.Serialize(scenarios, _json);
        config.ConfigVersion = Guid.NewGuid();
        config.UpdateTime    = DateTime.UtcNow;
        config.UpdatedFrom   = ConfigSource.Cloud;

        await db.SaveChangesAsync();

        var envelope = new ScenarioConfigEnvelope(config.ConfigVersion, config.UpdateTime, scenarios);
        await mqtt.PublishAsync(
            MqttHelper.GetMqttTopic(MqttTopics.CloudUserScenario, config.DeviceId),
            envelope,
            retainFlag: true);
    }

    private static List<MqttUserScenarioDto> Deserialize(string json) =>
        JsonSerializer.Deserialize<List<MqttUserScenarioDto>>(json, _json) ?? [];
}
