using SyncroInfraLayer.Enums;

namespace SyncroInfraLayer.Entities;

public class Device
{
    public Guid Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public Guid? CreatedByUser { get; set; }
    public int CityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public double? Longitude { get; set; }
    public double? Latitude { get; set; }

    public DeviceType Type { get; set; }
    public DeviceStatus Status { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? LastSeenAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public User? User { get; set; }
    public City City { get; set; } = null!;
}
