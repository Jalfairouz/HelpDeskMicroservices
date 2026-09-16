using UserManagementService.DTOs.Responses;
using UserManagementService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserManagementService.DTOs.Requests;
using UserManagementService.Models;
namespace UserManagementService.Controllers;

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
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var response = await _userService.GetByIdAsync(userId);

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

    [HttpGet("{userId:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<UserResponse>> GetById(Guid userId)
    {
        var user = await _userService.GetByIdForAdminAsync(userId);

        return user is null
            ? NotFound(new { message = "User not found." })
            : Ok(user);
    }

    [HttpPut("{userId:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<UserResponse>> Update(
        Guid userId,
        [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _userService.UpdateAsync(userId, request);

            return user is null
                ? NotFound(new { message = "User not found." })
                : Ok(user);
        }
        catch (ArgumentException exception)
        {
            return Conflict(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPatch("{userId:guid}/role")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<UserResponse>> ChangeRole(Guid userId, [FromBody] ChangeUserRoleRequest request)
    {
        try
        {
            var user = await _userService.ChangeRoleAsync(userId, request.Role);

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

    [HttpPatch("{userId:guid}/active-status")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<UserResponse>> ChangeActiveStatus(
        Guid userId,
        [FromBody] ChangeUserActiveStatusRequest request)
    {
        try
        {
            var user = await _userService.ChangeActiveStatusAsync(
                userId,
                request.IsActive!.Value);

            return user is null
                ? NotFound(new { message = "User not found." })
                : Ok(user);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("{userId:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Delete(Guid userId)
    {
        try
        {
            var deactivated = await _userService.DeactivateAsync(userId);

            return deactivated
                ? NoContent()
                : NotFound(new { message = "User not found." });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}