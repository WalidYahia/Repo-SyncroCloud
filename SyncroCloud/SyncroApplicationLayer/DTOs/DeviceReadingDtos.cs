namespace SyncroApplicationLayer.DTOs;

public record DeviceReadingDto(Guid Id, string DeviceId, Guid SensorId, DateTime RecordedAt, DateTime ReceivedAt, string Payload);

public record CreateDeviceReadingDto(string DeviceId, Guid SensorId, DateTime RecordedAt, string Payload);
