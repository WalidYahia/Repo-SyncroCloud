namespace SyncroInfraLayer.Entities;

public class RolePrivilege
{
    public Guid RoleId { get; set; }
    public Guid PrivilegeId { get; set; }

    public Privilege Privilege { get; set; } = null!;
}
