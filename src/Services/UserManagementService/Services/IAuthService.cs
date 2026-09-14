using IdentityService.DTOs.Requests;
using IdentityService.DTOs.Responses;

namespace IdentityService.Services;

public interface IAuthService
{
    Task<UserResponse> RegisterAsync(RegisterRequest request);

    Task<LoginResponse> LoginAsync(LoginRequest request);
}