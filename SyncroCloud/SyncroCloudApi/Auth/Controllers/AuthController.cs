using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.Auth.DTOs;
using SyncroApplicationLayer.Auth.Interfaces;

namespace SyncroCloudApi.Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var (success, errors) = await authService.RegisterAsync(dto);
        if (!success) return BadRequest(new { errors });
        return Ok(new { message = "User registered successfully" });
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDto>> Login(LoginDto dto)
    {
        var result = await authService.LoginAsync(dto);
        return result is null ? Unauthorized(new { error = "Invalid credentials" }) : Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponseDto>> Refresh(RefreshTokenDto dto)
    {
        var result = await authService.RefreshAsync(dto.RefreshToken);
        return result is null ? Unauthorized(new { error = "Invalid or expired refresh token" }) : Ok(result);
    }

    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(RefreshTokenDto dto)
    {
        var revoked = await authService.RevokeAsync(dto.RefreshToken);
        return revoked ? NoContent() : BadRequest(new { error = "Token not found or already revoked" });
    }
}
