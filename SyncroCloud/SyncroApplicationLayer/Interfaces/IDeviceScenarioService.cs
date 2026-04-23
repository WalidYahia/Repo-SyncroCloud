using SyncroApplicationLayer.DTOs;

namespace SyncroApplicationLayer.Interfaces;

public interface IDeviceScenarioService
{
    Task<List<DeviceScenarioDto>> GetByDeviceAsync(Guid deviceId);
    Task<DeviceScenarioDto?> GetByIdAsync(Guid deviceId, Guid scenarioId);
    Task<DeviceScenarioDto> UpsertAsync(Guid scenarioId, UpsertDeviceScenarioDto dto);
    Task<bool> DeleteAsync(Guid deviceId, Guid scenarioId);
    Task<int> DeleteAllByDeviceAsync(Guid deviceId);
}
