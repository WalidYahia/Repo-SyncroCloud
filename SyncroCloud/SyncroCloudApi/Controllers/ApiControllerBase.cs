using Microsoft.AspNetCore.Mvc;

namespace SyncroCloudApi.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult ResourceNotFound(string resource, object? id = null) =>
        NotFound(new { message = id is null ? $"{resource} not found." : $"{resource} with id '{id}' not found." });

    protected IActionResult ResourceConflict(string message) =>
        Conflict(new { message });

    protected IActionResult BadInput(string message) =>
        BadRequest(new { message });
}
