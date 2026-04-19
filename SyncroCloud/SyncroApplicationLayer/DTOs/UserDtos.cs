namespace SyncroApplicationLayer.DTOs;

public record UserDto(Guid Id, string Email, string FirstName, string LastName, DateTime CreatedAt, bool IsActive);

public record CreateUserDto(string Email, string FirstName, string LastName);

public record UpdateUserDto(string Email, string FirstName, string LastName, bool IsActive);
