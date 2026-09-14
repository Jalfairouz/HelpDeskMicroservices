using IdentityService.DTOs.Requests;
using IdentityService.DTOs.Responses;
using IdentityService.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(
        [FromBody] RegisterRequest request)
    {
        try
        {
            var response =
                await _authService.RegisterAsync(request);

            return StatusCode(
                StatusCodes.Status201Created,
                response);
        }
        catch (ArgumentException exception)
        {
            return Conflict(new
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
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
    [FromBody] LoginRequest request)
    {
        try
        {
            var response =
                await _authService.LoginAsync(request);

            return Ok(response);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(new
            {
                message = exception.Message
            });
        }
    }
}