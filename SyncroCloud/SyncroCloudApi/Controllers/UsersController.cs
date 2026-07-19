using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Identity;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class UsersController(IUserService service, RoleManager<AppRole> roleManager) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> GetAll() =>
        Ok(await service.GetAllAsync());

    [HttpGet("roles")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin}")]
    public async Task<IActionResult> GetRoles() =>
        Ok(await service.GetRolesAsync());

    [HttpGet("tenant/{tenantId:guid}")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin}")]
    public async Task<IActionResult> GetByTenant(Guid tenantId)
    {
        if (!await CanManageTenantAsync(tenantId)) return Forbid();
        return Ok(await service.GetByTenantAsync(tenantId));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        if (!await CanManageUserAsync(id)) return Forbid();
        var result = await service.GetByIdAsync(id);
        return result is null ? ResourceNotFound("User", id) : Ok(result);
    }

    [HttpGet("{id:guid}/tenants")]
    public async Task<IActionResult> GetTenants(Guid id)
    {
        if (!await CanManageUserAsync(id)) return Forbid();
        return Ok(await service.GetTenantsAsync(id));
    }

    [HttpPost]
    //[Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin}")]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        if (!User.IsInRole(AppRoles.SuperAdmin))
        {
            var role = await roleManager.FindByIdAsync(dto.RoleId.ToString());

            // TenantAdmins may only grant User or TenantAdmin — never SuperAdmin.
            if (role is null || role.Name == AppRoles.SuperAdmin) return Forbid();

            // TenantAdmins may only create users in tenants they manage.
            foreach (var tenantId in dto.TenantIds)
                if (!await CanManageTenantAsync(tenantId)) return Forbid();
        }

        var (success, user, errors) = await service.CreateAsync(dto);
        if (!success) return BadInput(string.Join("; ", errors));
        return CreatedAtAction(nameof(GetById), new { id = user!.Id }, user);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateUserDto dto)
    {
        if (!await CanManageUserAsync(id)) return Forbid();
        var result = await service.UpdateAsync(id, dto);
        return result is null ? ResourceNotFound("User", id) : Ok(result);
    }

    [HttpPatch("{id:guid}/role")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin}")]
    public async Task<IActionResult> UpdateRole(Guid id, UpdateUserRoleDto dto)
    {
        if (!await CanManageUserAsync(id)) return Forbid();

        if (!User.IsInRole(AppRoles.SuperAdmin))
        {
            // TenantAdmins may only grant User or TenantAdmin — never SuperAdmin.
            var targetRole = await roleManager.FindByIdAsync(dto.RoleId.ToString());
            if (targetRole is null || targetRole.Name == AppRoles.SuperAdmin)
                return Forbid();
        }

        var result = await service.UpdateRoleAsync(id, dto.RoleId);
        return result is null ? ResourceNotFound("User or Role") : Ok(result);
    }

    [HttpPost("{id:guid}/tenants/{tenantId:guid}")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin}")]
    public async Task<IActionResult> AddToTenant(Guid id, Guid tenantId)
    {
        if (!await CanManageTenantAsync(tenantId)) return Forbid();
        var added = await service.AddToTenantAsync(id, tenantId);
        return added ? NoContent() : ResourceNotFound("User or Tenant");
    }

    [HttpDelete("{id:guid}/tenants/{tenantId:guid}")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin}")]
    public async Task<IActionResult> RemoveFromTenant(Guid id, Guid tenantId)
    {
        if (!await CanManageTenantAsync(tenantId)) return Forbid();
        var removed = await service.RemoveFromTenantAsync(id, tenantId);
        return removed ? NoContent() : ResourceNotFound("User-Tenant association");
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await service.DeleteAsync(id);
        return deleted ? NoContent() : ResourceNotFound("User", id);
    }

    // ── Authorization helpers ───────────────────────────────────

    private async Task<bool> CanManageTenantAsync(Guid tenantId)
    {
        if (User.IsInRole(AppRoles.SuperAdmin)) return true;
        if (!User.IsInRole(AppRoles.TenantAdmin)) return false;
        var tenants = await service.GetTenantsAsync(CurrentUserId);
        return tenants.Any(t => t.Id == tenantId);
    }

    private async Task<bool> CanManageUserAsync(Guid targetUserId)
    {
        if (CurrentUserId == targetUserId) return true;
        if (User.IsInRole(AppRoles.SuperAdmin)) return true;
        if (!User.IsInRole(AppRoles.TenantAdmin)) return false;

        var callerTenants = (await service.GetTenantsAsync(CurrentUserId)).Select(t => t.Id).ToHashSet();
        var targetTenants = await service.GetTenantsAsync(targetUserId);
        return targetTenants.Any(t => callerTenants.Contains(t.Id));
    }
}
