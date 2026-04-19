using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Enums;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController(IDeviceService service) : ControllerBase
{
    [HttpGet("tenant/{tenantId:guid}")]
    public async Task<ActionResult<List<DeviceDto>>> GetByTenant(Guid tenantId) =>
        Ok(await service.GetByTenantAsync(tenantId));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DeviceDto>> GetById(Guid id)
    {
        var result = await service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<DeviceDto>> Create(CreateDeviceDto dto)
    {
        var result = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DeviceDto>> Update(Guid id, UpdateDeviceDto dto)
    {
        var result = await service.UpdateAsync(id, dto);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] DeviceStatus status)
    {
        var updated = await service.UpdateStatusAsync(id, status);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
