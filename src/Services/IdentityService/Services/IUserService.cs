using IdentityService.DTOs.Responses;

namespace IdentityService.Services;

public interface IUserService
{
    Task<UserResponse?> GetByIdAsync(Guid userId);

    Task<IEnumerable<UserResponse>> GetAllAsync();

    Task<UserResponse?> ChangeRoleAsync(
        Guid userId,
        string newRole);
}