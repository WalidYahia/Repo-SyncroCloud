using SyncroInfraLayer.Entities;

namespace SyncroInfraLayer.Identity;

public class DeviceApiKey
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public string KeyHash { get; set; } = string.Empty;  // SHA-256 of the raw key
    public string Prefix { get; set; } = string.Empty;   // first 8 chars for display
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;

    public Device Device { get; set; } = null!;
}
