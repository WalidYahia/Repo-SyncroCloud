using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController(ITenantService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TenantDto>>> GetAll() =>
        Ok(await service.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TenantDto>> GetById(Guid id)
    {
        var result = await service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TenantDto>> Create(CreateTenantDto dto)
    {
        var result = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TenantDto>> Update(Guid id, UpdateTenantDto dto)
    {
        var result = await service.UpdateAsync(id, dto);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
