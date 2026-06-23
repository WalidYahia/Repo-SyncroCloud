namespace SyncroApplicationLayer.DTOs;

public record DeviceReadingDto(Guid Id, string DeviceId, string DeviceSensorId, DateTime ReadingTime, DateTime PublishedAt, DateTime WriteTime, string Payload);

public record CreateDeviceReadingDto(string DeviceId, string DeviceSensorId, string Payload);
