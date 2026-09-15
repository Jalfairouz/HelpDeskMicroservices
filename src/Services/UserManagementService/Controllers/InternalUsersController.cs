
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementService.DTOs.Responses;
using UserManagementService.Services;

namespace UserManagementService.Controllers;

[ApiController]
[Route("api/internal/users")]
public class InternalUsersController : ControllerBase
{
    private readonly TechnicianQueryService _technicianQueryService;

    public InternalUsersController(TechnicianQueryService technicianQueryService)
    {
        _technicianQueryService = technicianQueryService;
    }

    [HttpGet("technicians")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<TechnicianResponse>>> GetTechnicians()
    {
        var technicians =  await _technicianQueryService.GetAllAsync();

        return Ok(technicians);
    }
}