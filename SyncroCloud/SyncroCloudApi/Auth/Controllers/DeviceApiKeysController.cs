using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.Auth.DTOs;
using SyncroApplicationLayer.Auth.Interfaces;

namespace SyncroCloudApi.Auth.Controllers;

[ApiController]
[Route("api/devices/{deviceId}/api-key")]
[Authorize]
public class DeviceApiKeysController(IApiKeyService apiKeyService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<GenerateApiKeyResponseDto>> Generate(string deviceId)
    {
        var result = await apiKeyService.GenerateAsync(deviceId);
        return Ok(result);
    }

    [HttpDelete]
    public async Task<IActionResult> Revoke(string deviceId)
    {
        var revoked = await apiKeyService.RevokeAsync(deviceId);
        return revoked ? NoContent() : NotFound();
    }
}
