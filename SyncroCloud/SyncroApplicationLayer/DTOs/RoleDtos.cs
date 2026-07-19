namespace SyncroApplicationLayer.DTOs;

public record PrivilegeDto(Guid Id, string Code, string Name);

public record RoleDetailDto(Guid Id, string Name, List<PrivilegeDto> Privileges);

public record CreateRoleDto(string Name, List<Guid> PrivilegeIds);

public record UpdateRolePrivilegesDto(List<Guid> PrivilegeIds);
