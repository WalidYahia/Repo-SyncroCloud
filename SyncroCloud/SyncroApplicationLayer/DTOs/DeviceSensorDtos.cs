using SyncroInfraLayer.Enums;

namespace SyncroApplicationLayer.DTOs;

public record DeviceSensorDto(
    string Id, string DeviceId, Guid SensorId,
    SwitchNo SwitchNo, string UnitId,
    int? Address, int? Port,
    string DisplayName, string Url,
    SensorType SensorType, int Protocol,
    string BaseUrl, string PortNo,
    string DataPath, string InfoPath, string InchingPath,
    int? SyncPeriodicity, bool EventChangeSync, double? EventChangeDelta,
    bool IsInInchingMode, int InchingModeWidthInMs,
    DateTime InstalledAt, bool IsActive,
    string? Notes, string? LastReading);

public record CreateDeviceSensorDto(
    string DeviceId, Guid SensorId,
    SwitchNo SwitchNo, string UnitId,
    int? Address, int? Port,
    string DisplayName, string Url,
    SensorType SensorType, int Protocol,
    string? BaseUrl, string? PortNo,
    string? DataPath, string? InfoPath, string? InchingPath,
    int? SyncPeriodicity, bool EventChangeSync, double? EventChangeDelta,
    bool IsInInchingMode, int InchingModeWidthInMs,
    Guid? InstalledById);

public record UpdateDeviceSensorDto(
    SwitchNo SwitchNo, string UnitId,
    int? Address, int? Port,
    string DisplayName, string Url,
    SensorType SensorType, int Protocol,
    string BaseUrl, string PortNo,
    string DataPath, string InfoPath, string InchingPath,
    int? SyncPeriodicity, bool EventChangeSync, double? EventChangeDelta,
    bool IsInInchingMode, int InchingModeWidthInMs,
    bool IsActive, string? Notes);

/// <summary>
/// Incoming device-side sensor sync payload (matches SmartGuardHub SensorConfig wire format).
/// </summary>
public record DeviceSensorSyncDto(
    string Id,
    string DeviceId,
    Guid SensorId,
    int SwitchNo,
    string UnitId,
    int? Address,
    int? Port,
    string DisplayName,
    string Url,
    int SensorType,
    int Protocol,
    string DataPath,
    string InfoPath,
    string InchingPath,
    int? SyncPeriodicity,
    bool EventChangeSync,
    double? EventChangeDelta,
    bool IsInInchingMode,
    int InchingModeWidthInMs,
    DateTime InstalledAt,
    bool IsActive,
    string? Notes);
