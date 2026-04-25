namespace SyncroInfraLayer.Entities;

public class DeviceReading
{
    public Guid Id { get; set; }
    public long DeviceSensorId { get; set; }
    public Guid DeviceId { get; set; }
    public Guid SensorId { get; set; }

    /// <summary>Timestamp reported by the device.</summary>
    public DateTime RecordedAt { get; set; }

    /// <summary>Timestamp when the server received the payload.</summary>
    public DateTime ReceivedAt { get; set; }

    /// <summary>JSONB — raw sensor measurement payload.</summary>
    public string Payload { get; set; } = "{}";

    public DeviceSensor DeviceSensor { get; set; } = null!;
}
