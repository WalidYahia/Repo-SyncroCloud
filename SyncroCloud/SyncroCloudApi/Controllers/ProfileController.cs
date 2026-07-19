using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SyncroApplicationLayer.DTOs;
using SyncroApplicationLayer.Interfaces;
using SyncroInfraLayer.Identity;

namespace SyncroCloudApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController(
    UserManager<AppUser> userManager,
    IRoleService roleService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMe()
    {
        var user = await userManager.FindByIdAsync(CurrentUserId.ToString());
        if (user is null) return Unauthorized();

        var roles      = (await userManager.GetRolesAsync(user)).ToList();
        var privileges = await roleService.GetUserPrivilegeCodesAsync(CurrentUserId);

        return Ok(new UserProfileDto(user.Id, user.PhoneNumber!, user.Email, user.FirstName, user.LastName, roles, privileges));
    }
}
