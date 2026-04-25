using SyncroInfraLayer.Enums;

namespace SyncroApplicationLayer.DTOs;

public record DeviceSensorDto(
    long Id, Guid DeviceId, Guid SensorId,
    SwitchNo SwitchNo, string UnitId,
    int? Address, int? Port,
    string DisplayName, string Url,
    UnitType UnitType, SensorType SensorType, int Protocol,
    int? SyncPeriodicity, bool EventChangeSync, double? EventChangeDelta,
    DateTime InstalledAt, bool IsActive,
    string? Notes, string? LastReading);

public record CreateDeviceSensorDto(
    Guid DeviceId, Guid SensorId,
    SwitchNo SwitchNo, string UnitId,
    int? Address, int? Port,
    string DisplayName, string Url,
    UnitType UnitType, SensorType SensorType, int Protocol,
    int? SyncPeriodicity, bool EventChangeSync, double? EventChangeDelta,
    Guid? InstalledById);

public record UpdateDeviceSensorDto(
    SwitchNo SwitchNo, string UnitId,
    int? Address, int? Port,
    string DisplayName, string Url,
    UnitType UnitType, SensorType SensorType, int Protocol,
    int? SyncPeriodicity, bool EventChangeSync, double? EventChangeDelta,
    bool IsActive, string? Notes);
