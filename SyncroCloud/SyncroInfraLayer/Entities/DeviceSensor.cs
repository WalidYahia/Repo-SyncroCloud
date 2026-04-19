using SyncroInfraLayer.Enums;

namespace SyncroInfraLayer.Entities;

public class DeviceSensor
{
    public Guid DeviceId { get; set; }
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

    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public string? LastReading { get; set; }
    public Device Device { get; set; } = null!;
    public Sensor Sensor { get; set; } = null!;
    public User? InstalledBy { get; set; }
    public ICollection<DeviceReading> Readings { get; set; } = [];
}
