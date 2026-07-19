using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Enums;
using SyncroInfraLayer.Identity;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController(IDeviceService service, IUserService userService) : ApiControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await service.GetByUserAsync(CurrentUserId));

    [HttpGet("tenant/{tenantId:guid}")]
    public async Task<IActionResult> GetByTenant(Guid tenantId) =>
        Ok(await service.GetByTenantAsync(tenantId));

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetByUser(Guid userId) =>
        Ok(await service.GetByUserAsync(userId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await service.GetByIdAsync(id);
        return result is null ? ResourceNotFound("Device", id) : Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(CreateDeviceDto dto)
    {
        try
        {
        var (success, device, error) = await service.CreateAsync(dto, CurrentUserId);
        if (!success) return BadInput(error!);
        return CreatedAtAction(nameof(GetById), new { id = device!.DeviceId }, device);
        }
        catch (Exception ex)
        {

            throw;
        }

    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateDeviceDto dto)
    {
        var result = await service.UpdateAsync(id, dto);
        return result is null ? ResourceNotFound("Device", id) : Ok(result);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] DeviceStatus status)
    {
        var updated = await service.UpdateStatusAsync(id, status);
        return updated ? NoContent() : ResourceNotFound("Device", id);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await service.DeleteAsync(id);
        return deleted ? NoContent() : ResourceNotFound("Device", id);
    }

    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin}")]
    [HttpGet("{deviceId}/users")]
    public async Task<IActionResult> GetDeviceUsers(string deviceId)
    {
        if (!await CanManageDeviceAsync(deviceId)) return Forbid();
        return Ok(await service.GetDeviceUsersAsync(deviceId));
    }

    [Authorize]
    [HttpGet("{deviceId}/users/{userId:guid}")]
    public async Task<IActionResult> GetUserLink(string deviceId, Guid userId)
    {
        // A user may read their own device-user link (and sensor permissions);
        // admins may read any user's link within their tenant scope.
        if (userId != CurrentUserId && !await CanManageDeviceAsync(deviceId)) return Forbid();
        var result = await service.GetUserLinkAsync(deviceId, userId);
        return result is null ? ResourceNotFound("DeviceUser link") : Ok(result);
    }

    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin}")]
    [HttpPost("{deviceId}/users/{userId:guid}")]
    public async Task<IActionResult> AssignUser(string deviceId, Guid userId)
    {
        if (!await CanManageDeviceAsync(deviceId)) return Forbid();
        var result = await service.AssignUserAsync(deviceId, userId);
        return Ok(result);
    }

    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin}")]
    [HttpDelete("{deviceId}/users/{userId:guid}")]
    public async Task<IActionResult> RemoveUser(string deviceId, Guid userId)
    {
        if (!await CanManageDeviceAsync(deviceId)) return Forbid();
        var removed = await service.RemoveUserAsync(deviceId, userId);
        return removed ? NoContent() : ResourceNotFound("DeviceUser link");
    }

    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin}")]
    [HttpPut("{deviceId}/users/{userId:guid}/permissions")]
    public async Task<IActionResult> UpdateSensorPermissions(string deviceId, Guid userId, List<SensorPermissionDto> permissions)
    {
        if (!await CanManageDeviceAsync(deviceId)) return Forbid();
        var result = await service.UpdateSensorPermissionsAsync(deviceId, userId, permissions);
        return result is null ? ResourceNotFound("DeviceUser link") : Ok(result);
    }

    // ── Authorization helpers ───────────────────────────────────

    private async Task<bool> CanManageDeviceAsync(string deviceId)
    {
        if (User.IsInRole(AppRoles.SuperAdmin)) return true;
        if (!User.IsInRole(AppRoles.TenantAdmin)) return false;

        var device = await service.GetByIdAsync(deviceId);
        if (device is null) return false;

        var tenants = await userService.GetTenantsAsync(CurrentUserId);
        return tenants.Any(t => t.Id == device.TenantId);
    }
}
