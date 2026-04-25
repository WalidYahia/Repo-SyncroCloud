using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeviceScenariosController(IDeviceScenarioService service) : ApiControllerBase
{
    [HttpGet("device/{deviceId:guid}")]
    public async Task<IActionResult> GetByDevice(Guid deviceId) =>
        Ok(await service.GetByDeviceAsync(deviceId));

    [HttpGet("device/{deviceId:guid}/{scenarioId:guid}")]
    public async Task<IActionResult> GetById(Guid deviceId, Guid scenarioId)
    {
        var result = await service.GetByIdAsync(deviceId, scenarioId);
        return result is null ? ResourceNotFound("Scenario", scenarioId) : Ok(result);
    }

    [HttpPut("{scenarioId:guid}")]
    public async Task<IActionResult> Upsert(Guid scenarioId, UpsertDeviceScenarioDto dto)
    {
        var result = await service.UpsertAsync(scenarioId, dto);
        return Ok(result);
    }

    [HttpDelete("device/{deviceId:guid}/{scenarioId:guid}")]
    public async Task<IActionResult> Delete(Guid deviceId, Guid scenarioId)
    {
        var deleted = await service.DeleteAsync(deviceId, scenarioId);
        return deleted ? NoContent() : ResourceNotFound("Scenario", scenarioId);
    }

    [HttpDelete("device/{deviceId:guid}")]
    public async Task<IActionResult> DeleteAll(Guid deviceId)
    {
        var count = await service.DeleteAllByDeviceAsync(deviceId);
        return Ok(new { deleted = count, message = $"{count} scenario(s) deleted." });
    }
}
