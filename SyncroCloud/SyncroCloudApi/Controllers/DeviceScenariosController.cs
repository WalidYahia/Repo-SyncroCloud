using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeviceScenariosController(IDeviceScenarioService service) : ApiControllerBase
{
    [HttpGet("device/{deviceId}")]
    public async Task<IActionResult> GetByDevice(string deviceId) =>
        Ok(await service.GetByDeviceAsync(deviceId));

    [HttpGet("device/{deviceId}/{scenarioId}")]
    public async Task<IActionResult> GetById(string deviceId, string scenarioId)
    {
        var result = await service.GetByIdAsync(deviceId, scenarioId);
        return result is null ? ResourceNotFound("Scenario", scenarioId) : Ok(result);
    }

    [HttpPost("device/{deviceId}")]
    public async Task<IActionResult> Create(string deviceId, [FromBody] MqttUserScenarioDto scenario)
    {
        var result = await service.CreateAsync(deviceId, scenario);
        return CreatedAtAction(nameof(GetById), new { deviceId, scenarioId = result.Id }, result);
    }

    [HttpPut("device/{deviceId}/{scenarioId}")]
    public async Task<IActionResult> Update(string deviceId, string scenarioId, [FromBody] MqttUserScenarioDto scenario)
    {
        var result = await service.UpdateAsync(deviceId, scenarioId, scenario);
        return result is null ? ResourceNotFound("Scenario", scenarioId) : Ok(result);
    }

    [HttpDelete("device/{deviceId}/{scenarioId}")]
    public async Task<IActionResult> Delete(string deviceId, string scenarioId)
    {
        var deleted = await service.DeleteAsync(deviceId, scenarioId);
        return deleted ? NoContent() : ResourceNotFound("Scenario", scenarioId);
    }
}
