using SyncroInfraLayer.Enums;

namespace SyncroInfraLayer.Entities;

public class Sensor
{
    public Guid SensorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public SensorType Type { get; set; }
    public ConnectionProtocol ConnectionProtocol { get; set; }

    public string BaseUrl { get; set; } = string.Empty;
    public string PortNo { get; set; } = string.Empty;
    public string DataPath { get; set; } = string.Empty;
    public string InfoPath { get; set; } = string.Empty;
    public string InchingPath { get; set; } = string.Empty;

    public int? SyncPeriodicity { get; set; }
    public bool EventChangeSync { get; set; }
    public double? EventChangeDelta { get; set; }

    public ICollection<DeviceSensor> DeviceSensors { get; set; } = [];
}
