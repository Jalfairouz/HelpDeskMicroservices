using UserManagementService.DTOs.Requests;
using UserManagementService.DTOs.Responses;

namespace UserManagementService.Services;

public interface IUserService
{
    Task<UserResponse?> GetByIdAsync(Guid userId);

    Task<UserResponse?> GetByIdForAdminAsync(Guid userId);

    Task<IEnumerable<UserResponse>> GetAllAsync();

    Task<UserResponse?> UpdateAsync(
        Guid userId,
        UpdateUserRequest request);

    Task<UserResponse?> ChangeRoleAsync(
        Guid userId,
        string newRole);

    Task<UserResponse?> ChangeActiveStatusAsync(
        Guid userId,
        bool isActive);

    Task<bool> DeactivateAsync(Guid userId);
}