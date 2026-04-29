using SyncroApplicationLayer.DTOs;

namespace SyncroApplicationLayer.Interfaces;

public interface IDeviceScenarioService
{
    Task<List<DeviceScenarioDto>> GetByDeviceAsync(string deviceId);
    Task<DeviceScenarioDto?> GetByIdAsync(string deviceId, Guid scenarioId);
    Task<DeviceScenarioDto> UpsertAsync(Guid scenarioId, UpsertDeviceScenarioDto dto);
    Task<bool> DeleteAsync(string deviceId, Guid scenarioId);
    Task<int> DeleteAllByDeviceAsync(string deviceId);
}
