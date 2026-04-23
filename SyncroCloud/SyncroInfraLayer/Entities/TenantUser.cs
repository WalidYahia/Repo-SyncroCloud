using SyncroInfraLayer.Identity;

namespace SyncroInfraLayer.Entities;

public class TenantUser
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}
