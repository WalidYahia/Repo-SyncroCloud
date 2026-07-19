using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Identity;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.TenantAdmin}")]
public class RolesController(IRoleService service) : ApiControllerBase
{
    [HttpGet("privileges")]
    public async Task<IActionResult> GetPrivileges() =>
        Ok(await service.GetAllPrivilegesAsync());

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await service.GetAllRolesAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await service.GetRoleAsync(id);
        return result is null ? ResourceNotFound("Role", id) : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRoleDto dto)
    {
        var (success, role, error) = await service.CreateRoleAsync(dto);
        if (!success) return BadInput(error!);
        return CreatedAtAction(nameof(GetById), new { id = role!.Id }, role);
    }

    [HttpPut("{id:guid}/privileges")]
    public async Task<IActionResult> UpdatePrivileges(Guid id, UpdateRolePrivilegesDto dto)
    {
        var updated = await service.UpdatePrivilegesAsync(id, dto);
        return updated ? Ok() : ResourceNotFound("Role", id);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await service.DeleteRoleAsync(id);
        return deleted ? NoContent() : ResourceNotFound("Role", id);
    }
}
