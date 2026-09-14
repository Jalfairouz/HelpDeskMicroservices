using UserManagementService.DTOs.Responses;
using UserManagementService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace UserManagementService.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserResponse?> GetByIdAsync(Guid userId)
    {
        var user =
            await _userManager.FindByIdAsync(userId.ToString());

        if (user is null || !user.IsActive)
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
            responses.Add(
                await CreateResponseAsync(user));
        }

        return responses;
    }

    public async Task<UserResponse?> ChangeRoleAsync(
        Guid userId,
        string newRole)
    {
        var allowedRoles = new[]
        {
            RoleNames.Employee,
            RoleNames.Technician
        };

        if (!allowedRoles.Contains(newRole))
        {
            throw new ArgumentException(
                "Role must be Employee or Technician.");
        }

        var user =
            await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return null;
        }

        if (await _userManager.IsInRoleAsync(
                user,
                RoleNames.Admin))
        {
            throw new InvalidOperationException(
                "Admin role cannot be changed.");
        }

        var currentRoles =
            await _userManager.GetRolesAsync(user);

        if (currentRoles.Contains(newRole))
        {
            return await CreateResponseAsync(user);
        }

        if (currentRoles.Count > 0)
        {
            var removeResult =
                await _userManager.RemoveFromRolesAsync(
                    user,
                    currentRoles);

            if (!removeResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Could not remove the current role.");
            }
        }

        var addResult =
            await _userManager.AddToRoleAsync(user, newRole);

        if (!addResult.Succeeded)
        {
            throw new InvalidOperationException(
                "Could not assign the new role.");
        }

        return await CreateResponseAsync(user);
    }

    private async Task<UserResponse> CreateResponseAsync(
        ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = roles.FirstOrDefault() ?? RoleNames.Employee
        };
    }
}