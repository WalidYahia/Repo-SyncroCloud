using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.Auth.DTOs;
using SyncroApplicationLayer.Auth.Interfaces;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Data;
using SyncroInfraLayer.Identity;

namespace SyncroApplicationLayer.Auth.Services;

public class AuthService(
    UserManager<AppUser> userManager,
    RoleManager<AppRole> roleManager,
    IUserService userService,
    SyncroDbContext db,
    TokenService tokenService) : IAuthService
{
    public async Task<(bool Success, IEnumerable<string> Errors)> RegisterAsync(RegisterDto dto)
    {
        // Self-service registration is always pinned to the "User" role — the DTO carries
        // no role field at all, so there is nothing here for a caller to escalate.
        var userRole = await roleManager.FindByNameAsync(AppRoles.User);
        if (userRole is null)
            return (false, ["Default user role is not configured."]);

        var request = new ProvisionUserRequest(dto.PhoneNumber, dto.Email, dto.Password, dto.FirstName, dto.LastName, dto.TenantId, userRole.Id);
        var (success, _, errors) = await userService.ProvisionAsync(request);
        return (success, errors);
    }

    public async Task<TokenResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.EmailOrPhone)
                   ?? await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == dto.EmailOrPhone);

        if (user is null || !user.IsActive) return null;

        var valid = await userManager.CheckPasswordAsync(user, dto.Password);
        if (!valid) return null;

        return await IssueTokensAsync(user);
    }

    public async Task<TokenResponseDto?> RefreshAsync(string refreshToken)
    {
        var stored = await db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (stored is null || stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
            return null;

        // rotate: revoke old, issue new
        stored.IsRevoked = true;
        stored.ReplacedByToken = tokenService.GenerateRefreshToken();

        var newRefresh = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = stored.UserId,
            Token = stored.ReplacedByToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = tokenService.RefreshTokenExpiry()
        };

        db.RefreshTokens.Add(newRefresh);
        await db.SaveChangesAsync();

        var roles = await userManager.GetRolesAsync(stored.User);
        var (accessToken, expiresAt) = tokenService.GenerateAccessToken(stored.User, roles);

        return new TokenResponseDto(accessToken, newRefresh.Token, expiresAt);
    }

    public async Task<bool> RevokeAsync(string refreshToken)
    {
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken);
        if (stored is null || stored.IsRevoked) return false;
        stored.IsRevoked = true;
        await db.SaveChangesAsync();
        return true;
    }

    private async Task<TokenResponseDto> IssueTokensAsync(AppUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user, roles);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = tokenService.GenerateRefreshToken(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = tokenService.RefreshTokenExpiry()
        };

        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync();

        return new TokenResponseDto(accessToken, refreshToken.Token, expiresAt);
    }
}
