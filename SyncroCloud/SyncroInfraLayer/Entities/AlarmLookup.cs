using SyncroInfraLayer.Enums;

namespace SyncroInfraLayer.Entities;

public class AlarmLookup
{
    public int Id { get; set; }
    public SensorType SensorType { get; set; }

    /// <summary>The measurement key this alarm applies to, e.g. "temperature", "value".</summary>
    public string MeasurementKey { get; set; } = string.Empty;

    public AlarmCondition Condition { get; set; }
    public double DefaultThreshold { get; set; }

    /// <summary>Upper bound — only used when Condition is Between.</summary>
    public double? DefaultThresholdMax { get; set; }

    public AlarmSeverity Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
