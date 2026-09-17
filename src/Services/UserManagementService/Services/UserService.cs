using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagementService.DTOs.Requests;
using UserManagementService.DTOs.Responses;
using UserManagementService.Models;

namespace UserManagementService.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request)
    {
        if (!IsAllowedRole(request.Role))
        {
            throw new InvalidOperationException("Role must be Employee or Technician.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            throw new ArgumentException("An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            Email = email,
            UserName = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(GetErrors(createResult));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            throw new InvalidOperationException(GetErrors(roleResult));
        }

        return await CreateResponseAsync(user);
    }

    public async Task<UserResponse?> GetByIdAsync(Guid userId, bool includeInactive = false)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return null;
        }
        if (!includeInactive && !user.IsActive)
        {
            return null;
        }
        return await CreateResponseAsync(user);
    }

    public async Task<IEnumerable<UserResponse>> GetAllAsync()
    {
        var users = await _userManager.Users
            .OrderBy(user => user.FirstName)
            .ToListAsync();

        var responses = new List<UserResponse>();

        foreach (var user in users)
        {
            responses.Add(await CreateResponseAsync(user));
        }

        return responses;
    }

    public async Task<UserResponse?> UpdateAsync(Guid userId, UpdateUserRequest request)
    {
        var user = await FindRegularUserAsync(userId);

        if (user is null)
        {
            return null;
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var userWithSameEmail = await _userManager.FindByEmailAsync(email);

        if (userWithSameEmail is not null && userWithSameEmail.Id != userId)
        {
            throw new ArgumentException("An account with this email already exists.");
        }

        user.Email = email;
        user.UserName = email;
        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(GetErrors(result));
        }

        return await CreateResponseAsync(user);
    }

    public async Task<UserResponse?> ChangeRoleAsync(Guid userId, string newRole)
    {
        if (!IsAllowedRole(newRole))
        {
            throw new InvalidOperationException("Role must be Employee or Technician.");
        }

        var user = await FindRegularUserAsync(userId);

        if (user is null)
        {
            return null;
        }
        var currentRoles = await _userManager.GetRolesAsync(user);

        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        var result = await _userManager.AddToRoleAsync(user, newRole);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(GetErrors(result));
        }

        return await CreateResponseAsync(user);
    }

    public async Task<UserResponse?> ChangeActiveStatusAsync(Guid userId, bool isActive)
    {
        var user = await FindRegularUserAsync(userId);

        if (user is null)
        {
            return null;
        }

        user.IsActive = isActive;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(GetErrors(result));
        }

        return await CreateResponseAsync(user);
    }

    // ----------  ---------- ---------- ----------

    private async Task<ApplicationUser?> FindRegularUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return null;
        }

        if (await _userManager.IsInRoleAsync(user, RoleNames.Admin))
        {
            throw new InvalidOperationException("Admin accounts cannot be modified here.");
        }

        return user;
    }

    private static bool IsAllowedRole(string role)
    {
        return role == RoleNames.Employee || role == RoleNames.Technician;
    }

    private async Task<UserResponse> CreateResponseAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = roles.FirstOrDefault() ?? RoleNames.Employee,
            IsActive = user.IsActive
        };
    }

    private static string GetErrors(IdentityResult result)
    {
        return string.Join(", ", result.Errors.Select(error => error.Description));
    }
}