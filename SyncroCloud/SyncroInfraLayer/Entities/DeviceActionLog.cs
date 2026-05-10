namespace SyncroInfraLayer.Entities;

public class DeviceActionLog
{
    public Guid   Id                 { get; set; }
    public string DeviceId           { get; set; } = string.Empty;
    public string InstalledSensorId  { get; set; } = string.Empty;
    public string Action             { get; set; } = string.Empty;
    public string Source             { get; set; } = string.Empty;
    public Guid?  TriggeredByUserId  { get; set; }
    public string Result             { get; set; } = string.Empty;
    public string? Notes             { get; set; }
    public DateTime Timestamp        { get; set; }
}
