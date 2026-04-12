namespace SyncroInfraLayer.Entities;

public class Device
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? UserId { get; set; }
    public int? CityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public DeviceType Type { get; set; }
    public DeviceStatus Status { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? LastSeenAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public User? User { get; set; }
    public City? City { get; set; }
}

public enum DeviceType
{
    SmartHome,
    Monitoring
}

public enum DeviceStatus
{
    Online,
    Offline,
    Maintenance
}
