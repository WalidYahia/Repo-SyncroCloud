namespace SyncroInfraLayer.Entities;

public class DeviceLatestReading
{
    public string DeviceSensorId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>Timestamp from payload "readingTime" (taken as-is, no conversion).</summary>
    public DateTime ReadingTime { get; set; }

    /// <summary>Timestamp from payload "publishedAt" (taken as-is, no conversion).</summary>
    public DateTime PublishedAt { get; set; }

    /// <summary>UTC timestamp set immediately before the record is written to the database.</summary>
    public DateTime WriteTime { get; set; }

    /// <summary>JSONB — raw sensor measurement payload.</summary>
    public string Payload { get; set; } = "{}";
}
