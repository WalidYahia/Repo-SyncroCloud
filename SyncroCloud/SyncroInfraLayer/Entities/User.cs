namespace SyncroInfraLayer.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    public ICollection<TenantUser> TenantUsers { get; set; } = [];
    public ICollection<Device> Devices { get; set; } = [];
}
