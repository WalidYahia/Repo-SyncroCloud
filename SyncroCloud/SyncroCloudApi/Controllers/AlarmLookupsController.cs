using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Enums;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlarmLookupsController(IAlarmLookupService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await service.GetAllAsync());

    [HttpGet("by-sensor-type/{type}")]
    public async Task<IActionResult> GetBySensorType(SensorType type) =>
        Ok(await service.GetBySensorTypeAsync(type));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await service.GetByIdAsync(id);
        return result is null ? ResourceNotFound("AlarmLookup", id) : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAlarmLookupDto dto)
    {
        var result = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateAlarmLookupDto dto)
    {
        var result = await service.UpdateAsync(id, dto);
        return result is null ? ResourceNotFound("AlarmLookup", id) : Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await service.DeleteAsync(id);
        return deleted ? NoContent() : ResourceNotFound("AlarmLookup", id);
    }
}
