using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll() =>
        Ok(await service.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id)
    {
        var result = await service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:guid}/tenants")]
    public async Task<ActionResult<List<TenantDto>>> GetTenants(Guid id) =>
        Ok(await service.GetTenantsAsync(id));

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserDto dto)
    {
        var result = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDto>> Update(Guid id, UpdateUserDto dto)
    {
        var result = await service.UpdateAsync(id, dto);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/tenants/{tenantId:guid}")]
    public async Task<IActionResult> AddToTenant(Guid id, Guid tenantId)
    {
        var added = await service.AddToTenantAsync(id, tenantId);
        return added ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}/tenants/{tenantId:guid}")]
    public async Task<IActionResult> RemoveFromTenant(Guid id, Guid tenantId)
    {
        var removed = await service.RemoveFromTenantAsync(id, tenantId);
        return removed ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
