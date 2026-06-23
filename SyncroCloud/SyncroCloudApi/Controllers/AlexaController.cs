using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Enums;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/alexa")]
[AllowAnonymous]
public class AlexaController(
    ISmartHomeService smartHome,
    IConfiguration config,
    ILogger<AlexaController> logger) : ControllerBase
{
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    [HttpPost("smart-home")]
    public async Task<IActionResult> HandleDirective([FromBody] AlexaRequest request)
    {
        var directive = request.Directive;
        var rawToken  = directive.Payload?.Scope?.Token ?? directive.Endpoint?.Scope.Token;
        var userId    = ValidateToken(rawToken);

        if (userId is null)
            return Unauthorized();

        var ns   = directive.Header.Namespace;
        var name = directive.Header.Name;

        try
        {
            var response = (ns, name) switch
            {
                ("Alexa.Discovery",     "Discover") => await HandleDiscovery(userId.Value),
                ("Alexa.PowerController", "TurnOn") => await HandlePower(directive, turnOn: true),
                ("Alexa.PowerController", "TurnOff")=> await HandlePower(directive, turnOn: false),
                ("Alexa.ReportState",   _)          => await HandleStateReport(directive),
                _                                   => BuildError(directive, "INVALID_DIRECTIVE",
                                                           $"Unsupported directive {ns}/{name}")
            };

            return Content(JsonSerializer.Serialize(response, _json), "application/json");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Alexa directive {Ns}/{Name} failed", ns, name);
            return Content(JsonSerializer.Serialize(
                BuildError(directive, "INTERNAL_ERROR", ex.Message), _json), "application/json");
        }
    }

    // ── Handlers ──────────────────────────────────────────────

    private async Task<object> HandleDiscovery(Guid userId)
    {
        var sensors = await smartHome.DiscoverAsync(userId);

        var endpoints = sensors.Select(s => new
        {
            endpointId       = s.Id,
            friendlyName     = s.DisplayName,
            description      = s.SensorType.ToString(),
            manufacturerName = "SyncroCloud",
            displayCategories = new[] { AlexaCategory(s.SensorType) },
            capabilities     = new object[]
            {
                new { type = "AlexaInterface", @interface = "Alexa", version = "3" },
                new
                {
                    type = "AlexaInterface",
                    @interface = "Alexa.PowerController",
                    version = "3",
                    properties = new
                    {
                        supported           = new[] { new { name = "powerState" } },
                        proactivelyReported = false,
                        retrievable         = true
                    }
                },
                new
                {
                    type = "AlexaInterface",
                    @interface = "Alexa.EndpointHealth",
                    version = "3",
                    properties = new
                    {
                        supported           = new[] { new { name = "connectivity" } },
                        proactivelyReported = false,
                        retrievable         = true
                    }
                }
            }
        });

        return new
        {
            @event = new
            {
                header  = MakeHeader("Alexa.Discovery", "Discover.Response", null, null),
                payload = new { endpoints }
            }
        };
    }

    private async Task<object> HandlePower(AlexaDirective directive, bool turnOn)
    {
        var endpointId = directive.Endpoint!.EndpointId;

        var ack = turnOn
            ? await smartHome.TurnOnAsync(endpointId, HttpContext.RequestAborted)
            : await smartHome.TurnOffAsync(endpointId, HttpContext.RequestAborted);

        if (ack.State != RemoteActionState.Ok)
            return BuildError(directive, "ENDPOINT_UNREACHABLE", ack.State.ToString());

        return new
        {
            context = new
            {
                properties = new object[]
                {
                    new
                    {
                        @namespace              = "Alexa.PowerController",
                        name                    = "powerState",
                        value                   = turnOn ? "ON" : "OFF",
                        timeOfSample            = DateTime.UtcNow,
                        uncertaintyInMilliseconds = 500
                    },
                    new
                    {
                        @namespace              = "Alexa.EndpointHealth",
                        name                    = "connectivity",
                        value                   = new { value = "OK" },
                        timeOfSample            = DateTime.UtcNow,
                        uncertaintyInMilliseconds = 500
                    }
                }
            },
            @event = new
            {
                header   = MakeHeader("Alexa", "Response",
                               directive.Header.MessageId, directive.Header.CorrelationToken),
                endpoint = new { endpointId },
                payload  = new { }
            }
        };
    }

    private async Task<object> HandleStateReport(AlexaDirective directive)
    {
        var endpointId = directive.Endpoint!.EndpointId;
        var sensor     = await smartHome.GetSensorAsync(endpointId);
        var powerState = IsOn(sensor?.LastReading) ? "ON" : "OFF";

        return new
        {
            context = new
            {
                properties = new[]
                {
                    new
                    {
                        @namespace              = "Alexa.PowerController",
                        name                    = "powerState",
                        value                   = powerState,
                        timeOfSample            = DateTime.UtcNow,
                        uncertaintyInMilliseconds = 0
                    }
                }
            },
            @event = new
            {
                header   = MakeHeader("Alexa", "StateReport",
                               directive.Header.MessageId, directive.Header.CorrelationToken),
                endpoint = new { endpointId },
                payload  = new { }
            }
        };
    }

    // ── Helpers ───────────────────────────────────────────────

    private static object BuildError(AlexaDirective directive, string type, string message) => new
    {
        @event = new
        {
            header   = MakeHeader("Alexa", "ErrorResponse",
                           directive.Header.MessageId, directive.Header.CorrelationToken),
            endpoint = directive.Endpoint is null ? null : (object)new { directive.Endpoint.EndpointId },
            payload  = new { type, message }
        }
    };

    private static object MakeHeader(string ns, string name, string? messageId, string? correlationToken) => new
    {
        @namespace       = ns,
        name,
        messageId        = messageId ?? Guid.NewGuid().ToString(),
        correlationToken,
        payloadVersion   = "3"
    };

    private static string AlexaCategory(SensorType type) => type switch
    {
        SensorType.SonOffMiniR3Swich => "SWITCH",
        SensorType.Light             => "LIGHT",
        _                            => "SMARTPLUG"
    };

    private static bool IsOn(string? reading) =>
        reading is not null &&
        (reading.Equals("true", StringComparison.OrdinalIgnoreCase) ||
         reading.Equals("1",    StringComparison.OrdinalIgnoreCase) ||
         reading.Equals("on",   StringComparison.OrdinalIgnoreCase));

    private Guid? ValidateToken(string? token)
    {
        if (string.IsNullOrEmpty(token)) return null;
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = new SymmetricSecurityKey(
                                               Encoding.UTF8.GetBytes(config["Jwt:Secret"]!)),
                ValidateIssuer   = config["Jwt:Issuer"]   is not null,
                ValidIssuer      = config["Jwt:Issuer"],
                ValidateAudience = config["Jwt:Audience"] is not null,
                ValidAudience    = config["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew        = TimeSpan.Zero
            }, out _);

            var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                   ?? principal.FindFirst("sub")?.Value;

            return Guid.TryParse(sub, out var id) ? id : null;
        }
        catch
        {
            return null;
        }
    }
}
