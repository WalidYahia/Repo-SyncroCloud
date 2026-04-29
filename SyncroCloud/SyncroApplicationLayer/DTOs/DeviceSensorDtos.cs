using SyncroInfraLayer.Enums;

namespace SyncroApplicationLayer.DTOs;

public record DeviceSensorDto(
    string Id, string DeviceId, Guid SensorId,
    SwitchNo SwitchNo, string UnitId,
    int? Address, int? Port,
    string DisplayName, string Url,
    UnitType UnitType, SensorType SensorType, int Protocol,
    int? SyncPeriodicity, bool EventChangeSync, double? EventChangeDelta,
    bool IsInInchingMode, int InchingModeWidthInMs,
    DateTime InstalledAt, bool IsActive,
    string? Notes, string? LastReading);

public record CreateDeviceSensorDto(
    string DeviceId, Guid SensorId,
    SwitchNo SwitchNo, string UnitId,
    int? Address, int? Port,
    string DisplayName, string Url,
    UnitType UnitType, SensorType SensorType, int Protocol,
    int? SyncPeriodicity, bool EventChangeSync, double? EventChangeDelta,
    bool IsInInchingMode, int InchingModeWidthInMs,
    Guid? InstalledById);

public record UpdateDeviceSensorDto(
    SwitchNo SwitchNo, string UnitId,
    int? Address, int? Port,
    string DisplayName, string Url,
    UnitType UnitType, SensorType SensorType, int Protocol,
    int? SyncPeriodicity, bool EventChangeSync, double? EventChangeDelta,
    bool IsInInchingMode, int InchingModeWidthInMs,
    bool IsActive, string? Notes);
