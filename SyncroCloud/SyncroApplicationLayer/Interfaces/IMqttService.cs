namespace SyncroApplicationLayer.Interfaces;

public interface IMqttService
{
    Task PublishCommandAsync(Guid deviceId, string action, object payload, CancellationToken ct = default);
    bool IsConnected { get; }
}
