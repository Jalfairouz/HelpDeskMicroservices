using UserManagementService.DTOs.Responses;

namespace UserManagementService.Services;

public interface IUserService
{
    Task<UserResponse?> GetByIdAsync(Guid userId);

    Task<IEnumerable<UserResponse>> GetAllAsync();

    Task<UserResponse?> ChangeRoleAsync(
        Guid userId,
        string newRole);
}