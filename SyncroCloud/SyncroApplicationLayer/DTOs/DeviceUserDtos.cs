using SyncroInfraLayer.Enums;

namespace SyncroApplicationLayer.DTOs;

public record SensorPermissionDto(string SensorId, SensorAccessLevel Access);

public record DeviceUserDto(string DeviceId, Guid UserId, DateTime LinkedAt, List<SensorPermissionDto> SensorPermissions);
