using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SyncroApplicationLayer.Auth.DTOs;
using SyncroApplicationLayer.Auth.Interfaces;
using SyncroInfraLayer.Data;
using SyncroInfraLayer.Identity;

namespace SyncroApplicationLayer.Auth.Services;

public class ApiKeyService(SyncroDbContext db) : IApiKeyService
{
    public async Task<GenerateApiKeyResponseDto> GenerateAsync(Guid deviceId)
    {
        // revoke existing key if any
        var existing = await db.DeviceApiKeys.FirstOrDefaultAsync(k => k.DeviceId == deviceId);
        if (existing is not null)
            db.DeviceApiKeys.Remove(existing);

        var rawKey = GenerateRawKey(deviceId);
        var prefix = rawKey[..8];
        var hash = HashKey(rawKey);

        var apiKey = new DeviceApiKey
        {
            Id = Guid.NewGuid(),
            DeviceId = deviceId,
            KeyHash = hash,
            Prefix = prefix,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        db.DeviceApiKeys.Add(apiKey);
        await db.SaveChangesAsync();

        return new GenerateApiKeyResponseDto(rawKey, prefix, apiKey.CreatedAt);
    }

    public async Task<Guid?> ValidateAsync(string apiKey)
    {
        var hash = HashKey(apiKey);
        var record = await db.DeviceApiKeys
            .FirstOrDefaultAsync(k => k.KeyHash == hash && k.IsActive);

        if (record is null) return null;
        if (record.ExpiresAt.HasValue && record.ExpiresAt < DateTime.UtcNow) return null;

        return record.DeviceId;
    }

    public async Task<bool> RevokeAsync(Guid deviceId)
    {
        var key = await db.DeviceApiKeys.FirstOrDefaultAsync(k => k.DeviceId == deviceId);
        if (key is null) return false;
        db.DeviceApiKeys.Remove(key);
        await db.SaveChangesAsync();
        return true;
    }

    private static string GenerateRawKey(Guid deviceId)
    {
        var random = RandomNumberGenerator.GetBytes(32);
        return $"sk_{deviceId:N}_{Convert.ToHexString(random).ToLower()}";
    }

    private static string HashKey(string key)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(bytes).ToLower();
    }
}
