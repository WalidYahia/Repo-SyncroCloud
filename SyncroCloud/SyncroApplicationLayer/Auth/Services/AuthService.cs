using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.Auth.DTOs;
using SyncroApplicationLayer.Auth.Interfaces;
using SyncroInfraLayer.Data;
using SyncroInfraLayer.Identity;

namespace SyncroApplicationLayer.Auth.Services;

public class AuthService(
    UserManager<AppUser> userManager,
    SyncroDbContext db,
    TokenService tokenService) : IAuthService
{
    public async Task<(bool Success, IEnumerable<string> Errors)> RegisterAsync(RegisterDto dto)
    {
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return (false, result.Errors.Select(e => e.Description));

        await userManager.AddToRoleAsync(user, dto.Role);
        return (true, []);
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
