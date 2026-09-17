using UserManagementService.DTOs.Responses;
using UserManagementService.Models;
using Microsoft.AspNetCore.Identity;

namespace UserManagementService.Services;

public class TechnicianQueryService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public TechnicianQueryService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IEnumerable<TechnicianResponse>> GetAllAsync()
    {
        var technicians = await _userManager.GetUsersInRoleAsync(RoleNames.Technician);

        return technicians.Select(user => new TechnicianResponse
        {
            Id = user.Id,
            DisplayName = $"{user.FirstName} {user.LastName}".Trim(),
            IsActive = user.IsActive
        }).OrderBy(user => user.DisplayName);
    }
}