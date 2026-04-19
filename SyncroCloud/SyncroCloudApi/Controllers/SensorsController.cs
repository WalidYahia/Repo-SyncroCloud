using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensorsController(ISensorService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SensorDto>>> GetAll() =>
        Ok(await service.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SensorDto>> GetById(Guid id)
    {
        var result = await service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<SensorDto>> Create(CreateSensorDto dto)
    {
        var result = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.SensorId }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SensorDto>> Update(Guid id, UpdateSensorDto dto)
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
