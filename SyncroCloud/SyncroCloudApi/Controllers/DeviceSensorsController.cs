using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeviceSensorsController(IDeviceSensorService service) : ApiControllerBase
{
    [HttpGet("device/{deviceId}")]
    public async Task<IActionResult> GetByDevice(string deviceId) =>
        Ok(await service.GetByDeviceAsync(deviceId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await service.GetByIdAsync(id);
        return result is null ? ResourceNotFound("DeviceSensor", id) : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Install(CreateDeviceSensorDto dto)
    {
        var result = await service.InstallAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateDeviceSensorDto dto)
    {
        var result = await service.UpdateAsync(id, dto);
        return result is null ? ResourceNotFound("DeviceSensor", id) : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Uninstall(string id)
    {
        var removed = await service.UninstallAsync(id);
        return removed ? NoContent() : ResourceNotFound("DeviceSensor", id);
    }
}
