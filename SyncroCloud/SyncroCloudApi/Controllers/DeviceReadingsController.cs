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
        [FromQuery] string deviceSensorId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        if (string.IsNullOrEmpty(deviceSensorId)) return BadInput("deviceSensorId is required.");
        return Ok(await service.GetAsync(deviceSensorId, from, to));
    }

    [HttpPost]
    public async Task<IActionResult> Add(CreateDeviceReadingDto dto)
    {
        var result = await service.AddAsync(dto);
        return CreatedAtAction(nameof(Get), new { deviceSensorId = result.DeviceSensorId }, result);
    }
}
