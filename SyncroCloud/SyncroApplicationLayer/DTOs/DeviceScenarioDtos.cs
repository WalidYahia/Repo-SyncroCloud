namespace SyncroApplicationLayer.DTOs;

public record DeviceScenarioDto(Guid Id, Guid DeviceId, string Payload, DateTime UpdatedAt);

public record UpsertDeviceScenarioDto(Guid DeviceId, string Payload);
