using SyncroApplicationLayer.DTOs;

namespace SyncroApplicationLayer.Interfaces;

public interface ISmartHomeService
{
    Task<IReadOnlyList<DeviceSensorDto>> DiscoverAsync(Guid userId);
    Task<DeviceSensorDto?> GetSensorAsync(string installedSensorId);
    Task<RemoteActionAckDto> TurnOnAsync(string installedSensorId, CancellationToken ct);
    Task<RemoteActionAckDto> TurnOffAsync(string installedSensorId, CancellationToken ct);
}
