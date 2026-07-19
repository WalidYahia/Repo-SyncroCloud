using SyncroApplicationLayer.DTOs;
using SyncroInfraLayer.Enums;

namespace SyncroApplicationLayer.Interfaces;

public interface IDeviceService
{
    Task<List<DeviceDto>> GetAllAsync();
    Task<List<DeviceDto>> GetByTenantAsync(Guid tenantId);
    Task<List<DeviceDto>> GetByUserAsync(Guid userId);
    Task<DeviceDto?> GetByIdAsync(string id);
    Task<(bool Success, DeviceDto? Device, string? Error)> CreateAsync(CreateDeviceDto dto, Guid createdByUserId);
    Task<List<DeviceUserDto>> GetDeviceUsersAsync(string deviceId);
    Task<DeviceUserDto?> GetUserLinkAsync(string deviceId, Guid userId);
    Task<DeviceUserDto> AssignUserAsync(string deviceId, Guid userId);
    Task<bool> RemoveUserAsync(string deviceId, Guid userId);
    Task<DeviceUserDto?> UpdateSensorPermissionsAsync(string deviceId, Guid userId, List<SensorPermissionDto> permissions);
    Task<DeviceDto?> UpdateAsync(string id, UpdateDeviceDto dto);
    Task<bool> UpdateStatusAsync(string id, DeviceStatus status);
    Task<bool> UpdateLastSeenAsync(string id, DateTime lastSeenAt);
    Task<bool> DeleteAsync(string id);
}
