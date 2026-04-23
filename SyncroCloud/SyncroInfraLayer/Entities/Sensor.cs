using SyncroInfraLayer.Enums;

namespace SyncroInfraLayer.Entities;

public class Sensor
{
    public Guid SensorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public UnitType UnitType { get; set; }
    public SensorType Type { get; set; }
    public ConnectionProtocol ConnectionProtocol { get; set; }

    public int? SyncPeriodicity { get; set; }
    public bool EventChangeSync { get; set; }
    public double? EventChangeDelta { get; set; }

    public ICollection<DeviceSensor> DeviceSensors { get; set; } = [];
}
