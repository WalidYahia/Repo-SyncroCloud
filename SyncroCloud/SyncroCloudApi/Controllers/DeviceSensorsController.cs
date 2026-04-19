using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeviceSensorsController(IDeviceSensorService service) : ControllerBase
{
    [HttpGet("device/{deviceId:guid}")]
    public async Task<ActionResult<List<DeviceSensorDto>>> GetByDevice(Guid deviceId) =>
        Ok(await service.GetByDeviceAsync(deviceId));

    [HttpGet("device/{deviceId:guid}/sensor/{sensorId:guid}")]
    public async Task<ActionResult<DeviceSensorDto>> GetById(Guid deviceId, Guid sensorId)
    {
        var result = await service.GetByIdAsync(deviceId, sensorId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<DeviceSensorDto>> Install(CreateDeviceSensorDto dto)
    {
        var result = await service.InstallAsync(dto);
        return CreatedAtAction(nameof(GetById), new { deviceId = result.DeviceId, sensorId = result.SensorId }, result);
    }

    [HttpPut("device/{deviceId:guid}/sensor/{sensorId:guid}")]
    public async Task<ActionResult<DeviceSensorDto>> Update(Guid deviceId, Guid sensorId, UpdateDeviceSensorDto dto)
    {
        var result = await service.UpdateAsync(deviceId, sensorId, dto);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("device/{deviceId:guid}/sensor/{sensorId:guid}/last-reading")]
    public async Task<IActionResult> UpdateLastReading(Guid deviceId, Guid sensorId, [FromBody] string json)
    {
        var updated = await service.UpdateLastReadingAsync(deviceId, sensorId, json);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("device/{deviceId:guid}/sensor/{sensorId:guid}")]
    public async Task<IActionResult> Uninstall(Guid deviceId, Guid sensorId)
    {
        var removed = await service.UninstallAsync(deviceId, sensorId);
        return removed ? NoContent() : NotFound();
    }
}
