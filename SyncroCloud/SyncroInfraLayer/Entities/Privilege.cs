namespace SyncroInfraLayer.Entities;

public class Privilege
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public ICollection<RolePrivilege> RolePrivileges { get; set; } = [];
}
