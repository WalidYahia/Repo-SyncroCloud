namespace SyncroApplicationLayer.DTOs;

public record TenantDto(Guid Id, string Name, DateTime CreatedAt, bool IsActive);

public record CreateTenantDto(string Name);

public record UpdateTenantDto(string Name, bool IsActive);
