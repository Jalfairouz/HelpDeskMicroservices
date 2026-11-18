using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementService.DTOs.External;
using UserManagementService.DTOs.Requests;
using UserManagementService.DTOs.Responses;
using UserManagementService.Services;

namespace UserManagementService.Controllers;

[ApiController]
[Route("api/internal/users")]
[AllowAnonymous] 
public class InternalUsersController : ControllerBase
{
    private readonly TechnicianQueryService _technicianQueryService;
    private readonly IUserService _userService;
    private readonly DirectoryQueryService _directoryQueryService;

    public InternalUsersController(
        TechnicianQueryService technicianQueryService,
        IUserService userService,
        DirectoryQueryService directoryQueryService)
    {
        _technicianQueryService = technicianQueryService;
        _userService = userService;
        _directoryQueryService = directoryQueryService;
    }

    [HttpGet("technicians")]
    public async Task<ActionResult<IEnumerable<TechnicianResponse>>> GetTechnicians()
    {
        return Ok(await _technicianQueryService.GetAllAsync());
    }

    [HttpGet("admins")]
    public async Task<ActionResult<IEnumerable<Guid>>> GetAdminIds()
    {
        return Ok(await _directoryQueryService.GetAdminIdsAsync());
    }

    [HttpPost("lookup")]
    public async Task<ActionResult<IEnumerable<UserNameResponse>>> Lookup( [FromBody] UserLookupRequest request)
    {
        return Ok(await _directoryQueryService.GetNamesAsync(request.Ids));
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<UserResponseExternal>> GetUserDetails(Guid userId)
    {
        var user = await _userService.GetUserDetails(userId);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }
}