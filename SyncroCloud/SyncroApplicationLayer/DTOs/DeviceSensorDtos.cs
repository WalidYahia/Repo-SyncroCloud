using SyncroInfraLayer.Enums;

namespace SyncroApplicationLayer.DTOs;

public record DeviceSensorDto(
    Guid DeviceId, Guid SensorId,
    SwitchNo SwitchNo, string UnitId,
    int? Address, int? Port,
    string DisplayName, string Url,
    UnitType UnitType, SensorType SensorType, int Protocol,
    DateTime InstalledAt, bool IsActive,
    string? Notes, string? LastReading);

public record CreateDeviceSensorDto(
    Guid DeviceId, Guid SensorId,
    SwitchNo SwitchNo, string UnitId,
    int? Address, int? Port,
    string DisplayName, string Url,
    UnitType UnitType, SensorType SensorType, int Protocol,
    Guid? InstalledById);

public record UpdateDeviceSensorDto(
    SwitchNo SwitchNo, string UnitId,
    int? Address, int? Port,
    string DisplayName, string Url,
    UnitType UnitType, SensorType SensorType, int Protocol,
    bool IsActive, string? Notes);
