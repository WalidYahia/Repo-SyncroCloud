using Microsoft.AspNetCore.Identity;
using SyncroInfraLayer.Entities;

namespace SyncroInfraLayer.Identity;

public class AppUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<TenantUser> TenantUsers { get; set; } = [];
    public ICollection<Device> Devices { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
