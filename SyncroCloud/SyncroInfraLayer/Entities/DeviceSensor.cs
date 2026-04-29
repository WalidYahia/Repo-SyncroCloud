using SyncroInfraLayer.Enums;

namespace SyncroInfraLayer.Entities;

public class DeviceSensor
{
    public string Id { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public Guid SensorId { get; set; }
    public Guid? InstalledById { get; set; }

    public SwitchNo SwitchNo { get; set; }
    public string UnitId { get; set; } = string.Empty; // Sonoff device ID
    public int? Address { get; set; }
    public int? Port { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public UnitType UnitType { get; set; }
    public SensorType SensorType { get; set; }
    public int Protocol { get; set; }
    public DateTime InstalledAt { get; set; }

    public int? SyncPeriodicity { get; set; }
    public bool EventChangeSync { get; set; }
    public double? EventChangeDelta { get; set; }
    public bool IsInInchingMode { get; set; }
    public int InchingModeWidthInMs { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public Device Device { get; set; } = null!;
    public Sensor Sensor { get; set; } = null!;
    public Identity.AppUser? InstalledBy { get; set; }
    public ICollection<DeviceReading> Readings { get; set; } = [];

    /// <summary>
    /// Builds the deterministic string PK.
    /// Null/empty slots use their lowercase field name as a literal placeholder.
    /// </summary>
    public static string ComputeId(
        string deviceId,
        SensorType sensorType,
        UnitType unitType,
        string unitId,
        SwitchNo switchNo,
        int? address,
        int? port)
    {
        var unitIdPart  = string.IsNullOrEmpty(unitId) ? "unitId"  : unitId;
        var switchPart  = switchNo == SwitchNo.Non     ? "switch"  : switchNo.ToString();
        var addressPart = address.HasValue             ? address.Value.ToString() : "address";
        var portPart    = port.HasValue                ? port.Value.ToString()    : "port";

        return $"{deviceId}_{sensorType}_{unitType}_{unitIdPart}_{switchPart}_{addressPart}_{portPart}";
    }
}
