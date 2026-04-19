using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeviceReadingsController(IDeviceReadingService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DeviceReadingDto>>> Get(
        [FromQuery] Guid deviceId,
        [FromQuery] Guid sensorId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to) =>
        Ok(await service.GetAsync(deviceId, sensorId, from, to));

    [HttpPost]
    public async Task<ActionResult<DeviceReadingDto>> Add(CreateDeviceReadingDto dto)
    {
        var result = await service.AddAsync(dto);
        return CreatedAtAction(nameof(Get), new { deviceId = result.DeviceId, sensorId = result.SensorId }, result);
    }
}
