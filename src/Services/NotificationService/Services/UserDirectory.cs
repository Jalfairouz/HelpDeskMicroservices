using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace NotificationService.Services;

public interface IUserDirectory
{
    Task<IReadOnlyDictionary<Guid, string>> GetNamesAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);

    Task<IReadOnlyList<Guid>> GetAdminIdsAsync(CancellationToken cancellationToken);
}

public static class UserDirectoryExtensions
{
    public static string NameOf(this IReadOnlyDictionary<Guid, string> names, Guid id)
        => names.TryGetValue(id, out var name) && !string.IsNullOrWhiteSpace(name)
            ? name
            : "Someone";
}

public sealed class UserDirectory : IUserDirectory
{
    private sealed record UserName(Guid Id, string DisplayName);

    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly ILogger<UserDirectory> _logger;

    public UserDirectory(HttpClient http, IMemoryCache cache, ILogger<UserDirectory> logger)
    {
        _http = http;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetNamesAsync( IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var result = new Dictionary<Guid, string>();
        var missing = new List<Guid>();

        foreach (var id in ids.Where(id => id != Guid.Empty).Distinct())
        {
            if (_cache.TryGetValue($"user-name:{id}", out string? cached) && cached is not null)
                result[id] = cached;
            else
                missing.Add(id);
        }

        if (missing.Count == 0)
            return result;

        try
        {
            var response = await _http.PostAsJsonAsync(
                "api/internal/users/lookup", new { ids = missing }, cancellationToken);

            response.EnsureSuccessStatusCode();

            var users = await response.Content
                .ReadFromJsonAsync<List<UserName>>(cancellationToken) ?? [];

            foreach (var user in users)
            {
                result[user.Id] = user.DisplayName;
                _cache.Set($"user-name:{user.Id}", user.DisplayName, TimeSpan.FromMinutes(10));
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "Could not resolve {Count} user names.", missing.Count);
        }

        return result;
    }

    public async Task<IReadOnlyList<Guid>> GetAdminIdsAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue("admin-ids", out IReadOnlyList<Guid>? cached) && cached is not null)
            return cached;

        try
        {
            var ids = await _http.GetFromJsonAsync<List<Guid>>(
                "api/internal/users/admins", cancellationToken) ?? [];

            _cache.Set("admin-ids", (IReadOnlyList<Guid>)ids, TimeSpan.FromMinutes(1));
            return ids;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "Could not load admin ids.");
            return [];
        }
    }
}