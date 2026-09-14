using UserManagementService.DTOs.Requests;
using UserManagementService.DTOs.Responses;

namespace UserManagementService.Services;

public interface IAuthService
{
    Task<UserResponse> RegisterAsync(RegisterRequest request);

    Task<LoginResponse> LoginAsync(LoginRequest request);
}