namespace SyncroInfraLayer.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    public ICollection<TenantUser> TenantUsers { get; set; } = [];
    public ICollection<Device> Devices { get; set; } = [];
}
