namespace SyncroApplicationLayer.Interfaces;

public interface IMqttService
{
    bool IsConnected { get; }
    Task PublishAsync(string topic, object payload, bool retainFlag, CancellationToken ct = default);
    Task PublishCommandAsync(Guid deviceId, string action, object payload, CancellationToken ct = default);
}
