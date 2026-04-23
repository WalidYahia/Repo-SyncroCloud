using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.Auth.DTOs;
using SyncroApplicationLayer.Auth.Interfaces;

namespace SyncroCloudApi.Auth.Controllers;

[ApiController]
[Route("api/devices/{deviceId:guid}/api-key")]
[Authorize]
public class DeviceApiKeysController(IApiKeyService apiKeyService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<GenerateApiKeyResponseDto>> Generate(Guid deviceId)
    {
        var result = await apiKeyService.GenerateAsync(deviceId);
        return Ok(result);
    }

    [HttpDelete]
    public async Task<IActionResult> Revoke(Guid deviceId)
    {
        var revoked = await apiKeyService.RevokeAsync(deviceId);
        return revoked ? NoContent() : NotFound();
    }
}
