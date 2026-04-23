namespace SyncroInfraLayer.Entities;

public class DeviceScenario
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public string Payload { get; set; } = "{}";  // jsonb — full UserScenario object
    public DateTime UpdatedAt { get; set; }

    public Device Device { get; set; } = null!;
}
