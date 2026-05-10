using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.Interfaces;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/action-logs")]
public class DeviceActionLogsController(IDeviceActionLogService service) : ApiControllerBase
{
    [HttpGet("device/{deviceId}")]
    public async Task<IActionResult> GetByDevice(string deviceId, [FromQuery] int limit = 100) =>
        Ok(await service.GetByDeviceAsync(deviceId, limit));

    [HttpGet("sensor/{installedSensorId}")]
    public async Task<IActionResult> GetBySensor(string installedSensorId, [FromQuery] int limit = 100) =>
        Ok(await service.GetBySensorAsync(installedSensorId, limit));
}
