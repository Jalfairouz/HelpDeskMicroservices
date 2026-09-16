using UserManagementService.DTOs.Requests;
using UserManagementService.DTOs.Responses;
using UserManagementService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace UserManagementService.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserResponse?> GetByIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null || !user.IsActive)
        {
            return null;
        }

        return await CreateResponseAsync(user);
    }

    public async Task<UserResponse?> GetByIdForAdminAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        return user is null ? null : await CreateResponseAsync(user);
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
        var allowedRoles = new[]
        {
            RoleNames.Employee,
            RoleNames.Technician
        };

        var normalizedRole = allowedRoles.FirstOrDefault(role =>
            role.Equals(newRole.Trim(), StringComparison.OrdinalIgnoreCase));

        if (normalizedRole is null)
        {
            throw new ArgumentException("Role must be Employee or Technician.");
        }

        var user = await FindRegularUserAsync(userId);

        if (user is null)
        {
            return null;
        }

        var currentRoles =
            await _userManager.GetRolesAsync(user);

        if (currentRoles.Contains(normalizedRole))
        {
            return await CreateResponseAsync(user);
        }

        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!removeResult.Succeeded)
            {
                throw new InvalidOperationException(GetErrors(removeResult));
            }
        }

        var addResult = await _userManager.AddToRoleAsync(user, normalizedRole);

        if (!addResult.Succeeded)
        {
            if (currentRoles.Count > 0)
            {
                await _userManager.AddToRolesAsync(user, currentRoles);
            }

            throw new InvalidOperationException(GetErrors(addResult));
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

    public async Task<bool> DeactivateAsync(Guid userId)
    {
        var user = await FindRegularUserAsync(userId);

        if (user is null)
        {
            return false;
        }

        if (!user.IsActive)
        {
            return true;
        }

        user.IsActive = false;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(GetErrors(result));
        }

        return true;
    }

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
