namespace SyncroApplicationLayer.Auth.DTOs;

public record RegisterDto(string Email, string Password, string FirstName, string LastName, string? PhoneNumber = null, string Role = "User");

public record LoginDto(string EmailOrPhone, string Password);

public record TokenResponseDto(string AccessToken, string RefreshToken, DateTime ExpiresAt);

public record RefreshTokenDto(string RefreshToken);

public record GenerateApiKeyResponseDto(string ApiKey, string Prefix, DateTime CreatedAt);
