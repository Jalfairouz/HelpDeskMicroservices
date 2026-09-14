using IdentityService.DTOs.Responses;
using IdentityService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using IdentityService.DTOs.Requests;
using IdentityService.Models;
namespace IdentityService.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> GetMe()
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var response =
            await _userService.GetByIdAsync(userId);

        if (response is null)
        {
            return Unauthorized();
        }

        return Ok(response);
    }
    [HttpGet]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll()
    {
        var users = await _userService.GetAllAsync();

        return Ok(users);
    }

    [HttpPatch("{userId:guid}/role")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<UserResponse>> ChangeRole(
        Guid userId,
        [FromBody] ChangeUserRoleRequest request)
    {
        try
        {
            var user = await _userService.ChangeRoleAsync(
                userId,
                request.Role);

            if (user is null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return Ok(user);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }
}