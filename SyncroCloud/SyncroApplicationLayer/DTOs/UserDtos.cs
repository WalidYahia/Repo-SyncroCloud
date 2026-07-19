namespace SyncroApplicationLayer.DTOs;

public record UserDto(Guid Id, string PhoneNumber, string? Email, string FirstName, string LastName, DateTime CreatedAt, bool IsActive, IReadOnlyList<string> Roles);

public record CreateUserDto(string PhoneNumber, string Password, string FirstName, string LastName, List<Guid> TenantIds, Guid RoleId, string? Email = null);

public record UpdateUserDto(string? Email, string FirstName, string LastName, bool IsActive);

public record UpdateUserRoleDto(Guid RoleId);

public record RoleDto(Guid Id, string Name);

public record UserProfileDto(Guid UserId, string PhoneNumber, string? Email, string FirstName, string LastName, List<string> Roles, List<string> Privileges);

/// <summary>
/// Internal shape both self-service registration and admin user-creation map into,
/// so the actual provisioning logic (Identity user + role + tenant membership) lives in one place.
/// </summary>
public record ProvisionUserRequest(string PhoneNumber, string? Email, string Password, string FirstName, string LastName, Guid TenantId, Guid RoleId);
