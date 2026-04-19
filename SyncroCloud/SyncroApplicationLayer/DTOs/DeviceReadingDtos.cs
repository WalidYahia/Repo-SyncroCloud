namespace SyncroApplicationLayer.DTOs;

public record DeviceReadingDto(Guid Id, Guid DeviceId, Guid SensorId, DateTime RecordedAt, DateTime ReceivedAt, string Payload);

public record CreateDeviceReadingDto(Guid DeviceId, Guid SensorId, DateTime RecordedAt, string Payload);
