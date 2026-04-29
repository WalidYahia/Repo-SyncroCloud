using SyncroApplicationLayer.Auth.DTOs;

namespace SyncroApplicationLayer.Auth.Interfaces;

public interface IApiKeyService
{
    Task<GenerateApiKeyResponseDto> GenerateAsync(string deviceId);
    Task<string?> ValidateAsync(string apiKey);
    Task<bool> RevokeAsync(string deviceId);
}
