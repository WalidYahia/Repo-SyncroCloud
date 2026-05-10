namespace SyncroApplicationLayer.DTOs;

public record DeviceActionLogDto(
    Guid     Id,
    string   DeviceId,
    string   InstalledSensorId,
    string   Action,
    string   Source,
    Guid?    TriggeredByUserId,
    string   Result,
    string?  Notes,
    DateTime Timestamp);

public record CreateDeviceActionLogDto(
    string  DeviceId,
    string  InstalledSensorId,
    string  Action,
    string  Source,
    string  Result,
    string? Notes         = null,
    Guid?   TriggeredByUserId = null);
