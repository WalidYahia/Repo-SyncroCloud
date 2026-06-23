using SyncroInfraLayer.Enums;

namespace SyncroApplicationLayer.DTOs;

public record DeviceSensorDto(
    string Id, string DeviceId, Guid SensorId,
    SwitchNo SwitchNo, string UnitId,
    int? Address, int? Port,
    string DisplayName, string? Url,
    SensorType SensorType, int Protocol,
    string DataPath, string InfoPath, string InchingPath,
    int? SyncPeriodicity, bool EventChangeSync, double? EventChangeDelta, bool OnlySaveRecordOnChange,
    bool IsInInchingMode, int InchingModeWidthInMs,
    DateTime InstalledAt, bool IsActive,
    string? Notes, string? LastReading, DateTime? LastSeen);

public record CreateDeviceSensorDto(
    string DeviceId, Guid SensorId,
    SwitchNo SwitchNo, string UnitId,
    int? Address, int? Port,
    string DisplayName, string? Url,
    SensorType SensorType, int Protocol,
    string? DataPath, string? InfoPath, string? InchingPath,
    int? SyncPeriodicity, double? EventChangeDelta,
    bool IsInInchingMode, int InchingModeWidthInMs);

public record UpdateDeviceSensorDto(
    SwitchNo SwitchNo, string UnitId,
    int? Address, int? Port,
    string DisplayName, string? Url,
    SensorType SensorType, int Protocol,
    string DataPath, string InfoPath, string InchingPath,
    int? SyncPeriodicity, bool EventChangeSync, double? EventChangeDelta, bool OnlySaveRecordOnChange,
    bool IsInInchingMode, int InchingModeWidthInMs,
    bool IsActive, string? Notes);

public record UpdateSensorNameDto(string Name);

/// <summary>
/// Hub wire format — matches SmartGuardHub SensorConfig schema exactly.
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
    string? Url,
    int SensorType,
    int Protocol,
    string DataPath,
    string InfoPath,
    string InchingPath,
    int? SyncPeriodicity,
    bool EventChangeSync,
    double? EventChangeDelta,
    bool OnlySaveRecordOnChange,
    bool IsInInchingMode,
    int InchingModeWidthInMs,
    DateTime InstalledAt,
    bool IsActive,
    string? Notes);

public static class SensorIdHelper
{
    public static string ComputeId(
        string deviceId,
        SensorType sensorType,
        string unitId,
        SwitchNo switchNo,
        int? address,
        int? port)
    {
        var unitIdPart  = string.IsNullOrEmpty(unitId) ? "unitId"  : unitId;
        var switchPart  = switchNo == SwitchNo.Non     ? "switch"  : switchNo.ToString();
        var addressPart = address.HasValue             ? address.Value.ToString() : "address";
        var portPart    = port.HasValue                ? port.Value.ToString()    : "port";
        return $"{deviceId}_{sensorType}_{unitIdPart}_{switchPart}_{addressPart}_{portPart}";
    }
}

/// <summary>
/// Envelope used for both CloudSensorConfig (cloud → hub) and DeviceSensorConfig (hub → cloud).
/// </summary>
public record SensorConfigEnvelope(
    Guid ConfigVersion,
    DateTime UpdateTime,
    List<DeviceSensorSyncDto> Sensors);
