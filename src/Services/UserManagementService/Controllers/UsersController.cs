using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementService.DTOs.Requests;
using UserManagementService.DTOs.Responses;
using UserManagementService.Models;
using UserManagementService.Services;

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

        var user = await _userService.GetByIdAsync(userId);

        return user is null ? Unauthorized() : Ok(user);
    }

 
    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _userService.CreateAsync(request);

            return CreatedAtAction(nameof(GetById), new { userId = user.Id }, user);
        }
        catch (ArgumentException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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
        var user = await _userService.GetByIdAsync(userId, includeInactive: true);

        return user is null ? NotFound(new { message = "User not found." }) : Ok(user);
    }


    [HttpPut("{userId:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<UserResponse>> Update(Guid userId, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _userService.UpdateAsync(userId, request);

            return user is null ? NotFound(new { message = "User not found." }) : Ok(user);
        }
        catch (ArgumentException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpPatch("{userId:guid}/role")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<UserResponse>> ChangeRole(Guid userId, [FromBody] ChangeUserRoleRequest request)
    {
        try
        {
            var user = await _userService.ChangeRoleAsync(userId, request.Role);

            return user is null ? NotFound(new { message = "User not found." }) : Ok(user);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }




    [HttpPatch("{userId:guid}/active-status")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<UserResponse>> ChangeActiveStatus(Guid userId, [FromBody] ChangeUserActiveStatusRequest request)
    {
        try
        {
            var user = await _userService.ChangeActiveStatusAsync(userId, request.IsActive!.Value);

            return user is null ? NotFound(new { message = "User not found." }) : Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }



    [HttpDelete("{userId:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Delete(Guid userId)
    {
        try
        {
            var user = await _userService.ChangeActiveStatusAsync(userId, false);

            return user is null ? NotFound(new { message = "User not found." }) : NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}