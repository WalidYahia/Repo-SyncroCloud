using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Enums;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/google-home")]
[Authorize]
public class GoogleHomeController(
    ISmartHomeService smartHome,
    ILogger<GoogleHomeController> logger) : ControllerBase
{
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    [HttpPost("fulfillment")]
    public async Task<IActionResult> Fulfillment([FromBody] GoogleHomeRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var input = request.Inputs.FirstOrDefault();
        if (input is null) return BadRequest();

        try
        {
            var response = input.Intent switch
            {
                "action.devices.SYNC"    => await HandleSync(request.RequestId, userId.Value),
                "action.devices.EXECUTE" => await HandleExecute(request.RequestId, input),
                "action.devices.QUERY"   => await HandleQuery(request.RequestId, input),
                _ => (object)new { requestId = request.RequestId,
                                   payload   = new { errorCode = "notSupported" } }
            };

            return Content(JsonSerializer.Serialize(response, _json), "application/json");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Google Home intent {Intent} failed", input.Intent);
            return StatusCode(500);
        }
    }

    // ── Handlers ──────────────────────────────────────────────

    private async Task<object> HandleSync(string requestId, Guid userId)
    {
        var sensors = await smartHome.DiscoverAsync(userId);

        var devices = sensors.Select(s => new
        {
            id              = s.Id,
            type            = "action.devices.types.OUTLET",
            traits          = new[] { "action.devices.traits.OnOff" },
            name            = new { name = s.DisplayName },
            willReportState = false,
            deviceInfo      = new { manufacturer = "SyncroCloud", model = s.SensorType.ToString() }
        });

        return new
        {
            requestId,
            payload = new
            {
                agentUserId = userId.ToString(),
                devices
            }
        };
    }

    private async Task<object> HandleExecute(string requestId, GoogleHomeInput input)
    {
        var successes = new List<string>();
        var failures  = new List<string>();
        bool lastTurnOn = false;

        foreach (var command in input.Payload?.Commands ?? [])
        {
            foreach (var execution in command.Execution)
            {
                var turnOn = ParseOnOff(execution);
                if (turnOn is null) continue;

                lastTurnOn = turnOn.Value;

                foreach (var device in command.Devices)
                {
                    try
                    {
                        var ack = turnOn.Value
                            ? await smartHome.TurnOnAsync(device.Id, HttpContext.RequestAborted)
                            : await smartHome.TurnOffAsync(device.Id, HttpContext.RequestAborted);

                        (ack.State == RemoteActionState.Ok ? successes : failures).Add(device.Id);
                    }
                    catch
                    {
                        failures.Add(device.Id);
                    }
                }
            }
        }

        var results = new List<object>();
        if (successes.Count > 0)
            results.Add(new { ids = successes, status = "SUCCESS",
                              states = new { on = lastTurnOn, online = true } });
        if (failures.Count > 0)
            results.Add(new { ids = failures, status = "ERROR", errorCode = "deviceOffline" });

        return new { requestId, payload = new { commands = results } };
    }

    private async Task<object> HandleQuery(string requestId, GoogleHomeInput input)
    {
        var states = new Dictionary<string, object>();

        foreach (var device in input.Payload?.Devices ?? [])
        {
            var sensor = await smartHome.GetSensorAsync(device.Id);
            states[device.Id] = sensor is null
                ? new { online = false }
                : (object)new { on = IsOn(sensor.LastReading), online = true };
        }

        return new { requestId, payload = new { devices = states } };
    }

    // ── Helpers ───────────────────────────────────────────────

    // Parses the OnOff execution command; returns null for unsupported commands.
    private static bool? ParseOnOff(GoogleHomeExecution execution)
    {
        if (execution.Command != "action.devices.commands.OnOff") return null;
        if (execution.Params?.TryGetValue("on", out var raw) != true) return null;

        return raw switch
        {
            bool b                                        => b,
            JsonElement { ValueKind: JsonValueKind.True } => true,
            JsonElement { ValueKind: JsonValueKind.False }=> false,
            _                                             => null
        };
    }

    private static bool IsOn(string? reading) =>
        reading is not null &&
        (reading.Equals("true", StringComparison.OrdinalIgnoreCase) ||
         reading.Equals("1",    StringComparison.OrdinalIgnoreCase) ||
         reading.Equals("on",   StringComparison.OrdinalIgnoreCase));

    private Guid? GetUserId()
    {
        var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
               ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
