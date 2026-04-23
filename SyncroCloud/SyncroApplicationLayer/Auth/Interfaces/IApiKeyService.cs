using SyncroApplicationLayer.Auth.DTOs;

namespace SyncroApplicationLayer.Auth.Interfaces;

public interface IApiKeyService
{
    Task<GenerateApiKeyResponseDto> GenerateAsync(Guid deviceId);
    Task<Guid?> ValidateAsync(string apiKey);
    Task<bool> RevokeAsync(Guid deviceId);
}
