using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeviceReadingsController(IDeviceReadingService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string deviceId,
        [FromQuery] Guid sensorId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        if (string.IsNullOrEmpty(deviceId)) return BadInput("deviceId is required.");
        if (sensorId == Guid.Empty) return BadInput("sensorId is required.");
        return Ok(await service.GetAsync(deviceId, sensorId, from, to));
    }

    [HttpPost]
    public async Task<IActionResult> Add(CreateDeviceReadingDto dto)
    {
        var result = await service.AddAsync(dto);
        return CreatedAtAction(nameof(Get), new { deviceId = result.DeviceId, sensorId = result.SensorId }, result);
    }
}
