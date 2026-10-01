
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementService.DTOs.External;
using UserManagementService.DTOs.Responses;
using UserManagementService.Services;

namespace UserManagementService.Controllers;

[ApiController]
[Route("api/internal/users")]
public class InternalUsersController : ControllerBase
{
    private readonly TechnicianQueryService _technicianQueryService;
    private readonly IUserService _userService;
    public InternalUsersController(TechnicianQueryService technicianQueryService, IUserService userService)
    {
        _technicianQueryService = technicianQueryService;
        _userService= userService;
    }

    [HttpGet("technicians")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<TechnicianResponse>>> GetTechnicians()
    {
        var technicians = await _technicianQueryService.GetAllAsync();

        return Ok(technicians);
    }
    [HttpGet("{userId}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<UserResponseExternal>>> GetUserDetails(Guid userId)
    {
        var user = await _userService.GetUserDetails(userId);
        return Ok(user);
    }
}