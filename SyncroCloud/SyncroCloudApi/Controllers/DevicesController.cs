using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Enums;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController(IDeviceService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await service.GetAllAsync());

    [HttpGet("tenant/{tenantId:guid}")]
    public async Task<IActionResult> GetByTenant(Guid tenantId) =>
        Ok(await service.GetByTenantAsync(tenantId));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await service.GetByIdAsync(id);
        return result is null ? ResourceNotFound("Device", id) : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateDeviceDto dto)
    {
        var result = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateDeviceDto dto)
    {
        var result = await service.UpdateAsync(id, dto);
        return result is null ? ResourceNotFound("Device", id) : Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] DeviceStatus status)
    {
        var updated = await service.UpdateStatusAsync(id, status);
        return updated ? NoContent() : ResourceNotFound("Device", id);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await service.DeleteAsync(id);
        return deleted ? NoContent() : ResourceNotFound("Device", id);
    }
}
