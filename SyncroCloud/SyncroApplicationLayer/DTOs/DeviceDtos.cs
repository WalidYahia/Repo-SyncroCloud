using SyncroInfraLayer.Enums;

namespace SyncroApplicationLayer.DTOs;

public record DeviceDto(
    string DeviceId, Guid TenantId, int CityId,
    string Name, string SerialNumber,
    double? Longitude, double? Latitude,
    DeviceType Type, DeviceStatus Status,
    DateTime RegisteredAt, DateTime? LastSeenAt);

public record CreateDeviceDto(
    string DeviceId, Guid TenantId, int CityId,
    string Name, string SerialNumber,
    DeviceType Type);

public record UpdateDeviceDto(
    string Name, int CityId,
    double? Longitude, double? Latitude,
    DeviceStatus Status);
