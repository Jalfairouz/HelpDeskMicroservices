using UserManagementService.DTOs.Requests;
using UserManagementService.DTOs.Responses;
using UserManagementService.Models;
using Microsoft.AspNetCore.Identity;

namespace UserManagementService.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TokenService _tokenService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    public AuthService(UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager, TokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    public async Task<UserResponse> RegisterAsync( RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var existingUser =
            await _userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            throw new ArgumentException(  "An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult =  await _userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(error => error.Description));

            throw new InvalidOperationException(errors);
        }

        var roleResult = await _userManager.AddToRoleAsync( user, RoleNames.Employee);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            var errors = string.Join( ", ", roleResult.Errors.Select(error => error.Description));

            throw new InvalidOperationException(errors);
        }

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = RoleNames.Employee
        };
    }
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _userManager.FindByEmailAsync(email);

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException( "Invalid email or password.");
        }

        var signInResult =
            await _signInManager.CheckPasswordSignInAsync( user, request.Password, lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            throw new UnauthorizedAccessException( "Account is temporarily locked.");
        }

        if (!signInResult.Succeeded)
        {
            throw new UnauthorizedAccessException( "Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        var role = roles.FirstOrDefault() ?? RoleNames.Employee;

        var tokenResult = _tokenService.GenerateToken(user, role);

        return new LoginResponse
        {
            AccessToken = tokenResult.Token,
            ExpiresAt = tokenResult.ExpiresAt
        };
    }
}