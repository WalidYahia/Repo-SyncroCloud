using SyncroInfraLayer.Enums;

namespace SyncroApplicationLayer.DTOs;

public record AlarmLookupDto(
    int Id, SensorType SensorType, string MeasurementKey,
    AlarmCondition Condition, double DefaultThreshold, double? DefaultThresholdMax,
    AlarmSeverity Severity, string Description, bool IsActive);

public record CreateAlarmLookupDto(
    SensorType SensorType, string MeasurementKey,
    AlarmCondition Condition, double DefaultThreshold, double? DefaultThresholdMax,
    AlarmSeverity Severity, string Description);

public record UpdateAlarmLookupDto(
    AlarmCondition Condition, double DefaultThreshold, double? DefaultThresholdMax,
    AlarmSeverity Severity, string Description, bool IsActive);
