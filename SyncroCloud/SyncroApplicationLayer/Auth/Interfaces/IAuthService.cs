using SyncroApplicationLayer.Auth.DTOs;

namespace SyncroApplicationLayer.Auth.Interfaces;

public interface IAuthService
{
    Task<(bool Success, IEnumerable<string> Errors)> RegisterAsync(RegisterDto dto);
    Task<TokenResponseDto?> LoginAsync(LoginDto dto);
    Task<TokenResponseDto?> RefreshAsync(string refreshToken);
    Task<bool> RevokeAsync(string refreshToken);
}
