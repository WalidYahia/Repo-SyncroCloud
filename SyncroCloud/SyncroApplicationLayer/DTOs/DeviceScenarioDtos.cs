namespace SyncroApplicationLayer.DTOs;

public record DeviceScenarioDto(Guid Id, string DeviceId, string Payload, DateTime UpdatedAt);

public record UpsertDeviceScenarioDto(string DeviceId, string Payload);
