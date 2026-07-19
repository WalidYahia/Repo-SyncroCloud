using SyncroInfraLayer.Identity;

namespace SyncroInfraLayer.Entities;

public class DeviceUser
{
    public string DeviceId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime LinkedAt { get; set; }

    /// <summary>JSONB — list of { SensorId, Access } entries this user may watch/control on this device.</summary>
    public string SensorPermissions { get; set; } = "[]";

    public Device Device { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}
