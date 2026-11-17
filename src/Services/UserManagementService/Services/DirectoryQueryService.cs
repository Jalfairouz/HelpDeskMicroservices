using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagementService.DTOs.Responses;
using UserManagementService.Models;

namespace UserManagementService.Services;

public class DirectoryQueryService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DirectoryQueryService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<UserNameResponse>> GetNamesAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.Distinct().Take(200).ToList();

        var users = await _userManager.Users
            .Where(user => idList.Contains(user.Id))
            .Select(user => new { user.Id, user.FirstName, user.LastName })
            .ToListAsync();

        return users
            .Select(user => new UserNameResponse(
                user.Id,
                $"{user.FirstName} {user.LastName}".Trim()))
            .ToList();
    }

    public async Task<List<Guid>> GetAdminIdsAsync()
    {
        var admins = await _userManager.GetUsersInRoleAsync(RoleNames.Admin);

        return admins
            .Where(admin => admin.IsActive)
            .Select(admin => admin.Id)
            .ToList();
    }
}