using SyncroApplicationLayer.DTOs;

namespace SyncroApplicationLayer.Interfaces;

public interface IMqttService
{
    bool IsConnected { get; }
    Task PublishAsync(string topic, object payload, bool retainFlag, CancellationToken ct = default);
    Task PublishCommandAsync(string deviceId, string action, object payload, CancellationToken ct = default);

    Task<RemoteActionAckDto> TurnOffUnitAsync(string hubId, string installedSensorId, CancellationToken ct = default);
    Task<RemoteActionAckDto> TurnOnUnitAsync(string hubId, string installedSensorId, CancellationToken ct = default);
    Task<RemoteActionAckDto> EnableInchingAsync(string hubId, string installedSensorId, string unitId, int inchingTimeInMs, CancellationToken ct = default);
    Task<RemoteActionAckDto> DisableInchingAsync(string hubId, string installedSensorId, string unitId, CancellationToken ct = default);
    Task<RemoteActionAckDto> UpdateUnitNameAsync(string hubId, string installedSensorId, string name, CancellationToken ct = default);
    Task<RemoteActionAckDto> SaveScenarioAsync(string hubId, MqttUserScenarioDto scenario, CancellationToken ct = default);
    Task<RemoteActionAckDto> DeleteScenarioAsync(string hubId, string scenarioId, CancellationToken ct = default);
}
