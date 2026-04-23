using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SyncroInfraLayer.Identity;

namespace SyncroApplicationLayer.Auth.Services;

public class TokenService(IConfiguration config)
{
    private readonly string _secret = config["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured");
    private readonly string _issuer = config["Jwt:Issuer"] ?? "SyncroCloud";
    private readonly string _audience = config["Jwt:Audience"] ?? "SyncroCloudClients";
    private readonly int _accessTokenMinutes = config.GetValue<int>("Jwt:AccessTokenExpiryMinutes", 15);
    private readonly int _refreshTokenDays = config.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7);

    public (string Token, DateTime ExpiresAt) GenerateAccessToken(AppUser user, IList<string> roles)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_accessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public DateTime RefreshTokenExpiry() => DateTime.UtcNow.AddDays(_refreshTokenDays);
}
