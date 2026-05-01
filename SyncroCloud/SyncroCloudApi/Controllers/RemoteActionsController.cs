using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Enums;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/remote-actions")]
public class RemoteActionsController(IMqttService mqtt) : ApiControllerBase
{
    // POST api/remote-actions/{hubId}/sensors/{installedSensorId}/turn-on
    [HttpPost("{hubId}/sensors/{installedSensorId}/turn-on")]
    public Task<IActionResult> TurnOn(string hubId, string installedSensorId) =>
        ExecuteActionAsync(() => mqtt.TurnOnUnitAsync(hubId, installedSensorId, HttpContext.RequestAborted));

    // POST api/remote-actions/{hubId}/sensors/{installedSensorId}/turn-off
    [HttpPost("{hubId}/sensors/{installedSensorId}/turn-off")]
    public Task<IActionResult> TurnOff(string hubId, string installedSensorId) =>
        ExecuteActionAsync(() => mqtt.TurnOffUnitAsync(hubId, installedSensorId, HttpContext.RequestAborted));

    // POST api/remote-actions/{hubId}/sensors/{installedSensorId}/inching/enable
    [HttpPost("{hubId}/sensors/{installedSensorId}/inching/enable")]
    public Task<IActionResult> EnableInching(string hubId, string installedSensorId, [FromBody] EnableInchingRequest req) =>
        ExecuteActionAsync(() => mqtt.EnableInchingAsync(hubId, installedSensorId, req.UnitId, req.InchingTimeInMs, HttpContext.RequestAborted));

    // POST api/remote-actions/{hubId}/sensors/{installedSensorId}/inching/disable
    [HttpPost("{hubId}/sensors/{installedSensorId}/inching/disable")]
    public Task<IActionResult> DisableInching(string hubId, string installedSensorId, [FromBody] DisableInchingRequest req) =>
        ExecuteActionAsync(() => mqtt.DisableInchingAsync(hubId, installedSensorId, req.UnitId, HttpContext.RequestAborted));

    // PUT api/remote-actions/{hubId}/sensors/{installedSensorId}/name
    [HttpPut("{hubId}/sensors/{installedSensorId}/name")]
    public Task<IActionResult> UpdateName(string hubId, string installedSensorId, [FromBody] UpdateUnitNameRequest req) =>
        ExecuteActionAsync(() => mqtt.UpdateUnitNameAsync(hubId, installedSensorId, req.Name, HttpContext.RequestAborted));

    // PUT api/remote-actions/{hubId}/scenarios
    [HttpPut("{hubId}/scenarios")]
    public Task<IActionResult> SaveScenario(string hubId, [FromBody] MqttUserScenarioDto scenario) =>
        ExecuteActionAsync(() => mqtt.SaveScenarioAsync(hubId, scenario, HttpContext.RequestAborted));

    // DELETE api/remote-actions/{hubId}/scenarios/{scenarioId}
    [HttpDelete("{hubId}/scenarios/{scenarioId}")]
    public Task<IActionResult> DeleteScenario(string hubId, string scenarioId) =>
        ExecuteActionAsync(() => mqtt.DeleteScenarioAsync(hubId, scenarioId, HttpContext.RequestAborted));

    // ── helpers ───────────────────────────────────────────────

    private async Task<IActionResult> ExecuteActionAsync(Func<Task<RemoteActionAckDto>> action)
    {
        try
        {
            var ack = await action();
            return ack.State == RemoteActionState.Ok
                ? Ok(ack)
                : UnprocessableEntity(ack);
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
