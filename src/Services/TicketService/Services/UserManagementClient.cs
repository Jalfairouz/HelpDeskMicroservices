using System.Net.Http.Json;
using System.Text.Json;
using TicketService.DTOs.External;

namespace TicketService.Services;

public class UserManagementClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserManagementClient> _logger;

    public UserManagementClient( HttpClient httpClient, ILogger<UserManagementClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<TechnicianResponse>> GetTechniciansAsync()
    {
        try
        {
            var technicians = await _httpClient.GetFromJsonAsync<List<TechnicianResponse>>("api/internal/users/technicians");

            return technicians ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "UserManagementService is unavailable.");

            return [];
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "UserManagementService request timed out.");

            return [];
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Invalid technicians response.");

            return [];
        }
    }
    public async Task<UserResponse?> GetUserByIdAsync(Guid userId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<UserResponse>($"api/internal/users/{userId}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(
                ex, "UserManagementService is unavailable."
            );

            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(
                ex,"UserManagementService request timed out."
            );

            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(
                ex, "Invalid user response."
            );

            return null;
        }
    }
}