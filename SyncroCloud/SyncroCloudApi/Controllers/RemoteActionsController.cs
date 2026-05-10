using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Enums;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/remote-actions")]
public class RemoteActionsController(
    IMqttService           mqtt,
    IDeviceSensorService   sensorService,
    IDeviceActionLogService logService) : ApiControllerBase
{
    // POST api/remote-actions/{hubId}/sensors/{installedSensorId}/turn-on
    [HttpPost("{hubId}/sensors/{installedSensorId}/turn-on")]
    public Task<IActionResult> TurnOn(string hubId, string installedSensorId) =>
        ExecuteActionAsync(
            () => mqtt.TurnOnUnitAsync(hubId, installedSensorId, HttpContext.RequestAborted),
            async ack =>
            {
                if (ack.DevicePayload is { } p && p.TryGetProperty("lastReading", out var v))
                    await sensorService.UpdateLastReadingAsync(installedSensorId, v.GetRawText());
                await logService.LogAsync(new CreateDeviceActionLogDto(
                    hubId, installedSensorId, "TurnOn", "Remote", ack.State.ToString()));
            });

    // POST api/remote-actions/{hubId}/sensors/{installedSensorId}/turn-off
    [HttpPost("{hubId}/sensors/{installedSensorId}/turn-off")]
    public Task<IActionResult> TurnOff(string hubId, string installedSensorId) =>
        ExecuteActionAsync(
            () => mqtt.TurnOffUnitAsync(hubId, installedSensorId, HttpContext.RequestAborted),
            async ack =>
            {
                if (ack.DevicePayload is { } p && p.TryGetProperty("lastReading", out var v))
                    await sensorService.UpdateLastReadingAsync(installedSensorId, v.GetRawText());
                await logService.LogAsync(new CreateDeviceActionLogDto(
                    hubId, installedSensorId, "TurnOff", "Remote", ack.State.ToString()));
            });

    // POST api/remote-actions/{hubId}/sensors/{installedSensorId}/inching/enable
    [HttpPost("{hubId}/sensors/{installedSensorId}/inching/enable")]
    public Task<IActionResult> EnableInching(string hubId, string installedSensorId, [FromBody] EnableInchingRequest req) =>
        ExecuteActionAsync(
            () => mqtt.EnableInchingAsync(hubId, installedSensorId, req.UnitId, req.InchingTimeInMs, HttpContext.RequestAborted),
            async ack =>
            {
                await sensorService.UpdateInchingAsync(installedSensorId, true, req.InchingTimeInMs);
                await logService.LogAsync(new CreateDeviceActionLogDto(
                    hubId, installedSensorId, "EnableInching", "Remote", ack.State.ToString(),
                    Notes: $"InchingTimeMs:{req.InchingTimeInMs}"));
            });

    // POST api/remote-actions/{hubId}/sensors/{installedSensorId}/inching/disable
    [HttpPost("{hubId}/sensors/{installedSensorId}/inching/disable")]
    public Task<IActionResult> DisableInching(string hubId, string installedSensorId, [FromBody] DisableInchingRequest req) =>
        ExecuteActionAsync(
            () => mqtt.DisableInchingAsync(hubId, installedSensorId, req.UnitId, HttpContext.RequestAborted),
            async ack =>
            {
                await sensorService.UpdateInchingAsync(installedSensorId, false, 0);
                await logService.LogAsync(new CreateDeviceActionLogDto(
                    hubId, installedSensorId, "DisableInching", "Remote", ack.State.ToString()));
            });

    // PUT api/remote-actions/{hubId}/sensors/{installedSensorId}/name
    // DB-first: update immediately, push MQTT config to hub best-effort (no ack wait)
    [HttpPut("{hubId}/sensors/{installedSensorId}/name")]
    public async Task<IActionResult> UpdateName(string hubId, string installedSensorId, [FromBody] UpdateUnitNameRequest req)
    {
        var updated = await sensorService.UpdateDisplayNameAsync(installedSensorId, req.Name);
        if (!updated) return ResourceNotFound("DeviceSensor", installedSensorId);

        await logService.LogAsync(new CreateDeviceActionLogDto(
            hubId, installedSensorId, "UpdateDisplayName", "Remote", "Ok",
            Notes: req.Name));

        return NoContent();
    }

    // PUT api/remote-actions/{hubId}/scenarios
    [HttpPut("{hubId}/scenarios")]
    public Task<IActionResult> SaveScenario(string hubId, [FromBody] MqttUserScenarioDto scenario) =>
        ExecuteActionAsync(
            () => mqtt.SaveScenarioAsync(hubId, scenario, HttpContext.RequestAborted));

    // DELETE api/remote-actions/{hubId}/scenarios/{scenarioId}
    [HttpDelete("{hubId}/scenarios/{scenarioId}")]
    public Task<IActionResult> DeleteScenario(string hubId, string scenarioId) =>
        ExecuteActionAsync(
            () => mqtt.DeleteScenarioAsync(hubId, scenarioId, HttpContext.RequestAborted));

    // ── helpers ───────────────────────────────────────────────

    private async Task<IActionResult> ExecuteActionAsync(
        Func<Task<RemoteActionAckDto>> action,
        Func<RemoteActionAckDto, Task>? onSuccess = null)
    {
        try
        {
            var ack = await action();
            if (ack.State == RemoteActionState.Ok && onSuccess != null)
                await onSuccess(ack);
            return ack.State == RemoteActionState.Ok ? Ok(ack) : UnprocessableEntity(ack);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(503, new { message = ex.Message });
        }
        catch (OperationCanceledException)
        {
            return StatusCode(504, new { message = "Hub did not acknowledge the command within the timeout." });
        }
    }
}
