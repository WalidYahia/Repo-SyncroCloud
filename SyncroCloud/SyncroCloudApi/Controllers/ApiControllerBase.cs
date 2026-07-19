using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace SyncroCloudApi.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected Guid CurrentUserId =>
        Guid.TryParse(
            User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out var id) ? id : Guid.Empty;

    protected IActionResult ResourceNotFound(string resource, object? id = null) =>
        NotFound(new { message = id is null ? $"{resource} not found." : $"{resource} with id '{id}' not found." });

    protected IActionResult ResourceConflict(string message) =>
        Conflict(new { message });

    protected IActionResult BadInput(string message) =>
        BadRequest(new { message });
}
