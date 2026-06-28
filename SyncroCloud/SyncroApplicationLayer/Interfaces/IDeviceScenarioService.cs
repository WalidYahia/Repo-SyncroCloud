using SyncroApplicationLayer.DTOs;

namespace SyncroApplicationLayer.Interfaces;

public interface IDeviceScenarioService
{
    Task<List<MqttUserScenarioDto>> GetByDeviceAsync(string deviceId);
    Task<MqttUserScenarioDto?> GetByIdAsync(string deviceId, string scenarioId);
    Task<MqttUserScenarioDto> CreateAsync(string deviceId, MqttUserScenarioDto scenario);
    Task<MqttUserScenarioDto?> UpdateAsync(string deviceId, string scenarioId, MqttUserScenarioDto scenario);
    Task<bool> DeleteAsync(string deviceId, string scenarioId);
    Task SyncFromDeviceAsync(string deviceId, ScenarioConfigEnvelope envelope);
}
