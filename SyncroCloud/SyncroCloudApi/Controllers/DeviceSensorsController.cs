using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeviceSensorsController(IDeviceSensorService service) : ApiControllerBase
{
    [HttpGet("device/{deviceId:guid}")]
    public async Task<IActionResult> GetByDevice(Guid deviceId) =>
        Ok(await service.GetByDeviceAsync(deviceId));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
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

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, UpdateDeviceSensorDto dto)
    {
        var result = await service.UpdateAsync(id, dto);
        return result is null ? ResourceNotFound("DeviceSensor", id) : Ok(result);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Uninstall(long id)
    {
        var removed = await service.UninstallAsync(id);
        return removed ? NoContent() : ResourceNotFound("DeviceSensor", id);
    }
}
