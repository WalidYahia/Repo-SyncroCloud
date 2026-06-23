using SyncroInfraLayer.Enums;

namespace SyncroInfraLayer.Entities;

public class DeviceConfig
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public ConfigType ConfigType { get; set; }
    public Guid ConfigVersion { get; set; }
    public DateTime UpdateTime { get; set; }
    public string Config { get; set; } = "[]";
    public ConfigSource UpdatedFrom { get; set; }

    public Device Device { get; set; } = null!;
}
