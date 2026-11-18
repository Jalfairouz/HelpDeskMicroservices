using Microsoft.Extensions.Caching.Memory;
using TicketService.DTOs.Responses;

namespace TicketService.Services;

public interface IUserNameResolver
{
    Task<IReadOnlyDictionary<Guid, string>> ResolveAsync(IEnumerable<Guid?> ids);

    Task EnrichAsync(IEnumerable<TicketResponse> tickets);

    Task EnrichAsync(IEnumerable<CommentResponse> comments);
}

public sealed class UserNameResolver : IUserNameResolver
{
    public const string UnknownUser = "Unknown user";
    public const string SystemName = "System";

    private const int LookupBatchSize = 200;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    private readonly UserManagementClient _client;
    private readonly IMemoryCache _cache;

    public UserNameResolver(UserManagementClient client, IMemoryCache cache)
    {
        _client = client;
        _cache = cache;
    }

    public async Task<IReadOnlyDictionary<Guid, string>> ResolveAsync(IEnumerable<Guid?> ids)
    {
        var result = new Dictionary<Guid, string>();
        var missing = new List<Guid>();

        foreach (var id in ids
                     .Where(id => id.HasValue && id.Value != Guid.Empty)
                     .Select(id => id!.Value)
                     .Distinct())
        {
            if (_cache.TryGetValue(CacheKey(id), out string? cached) && cached is not null)
            {
                result[id] = cached;
            }
            else
            {
                missing.Add(id);
            }
        }

        foreach (var batch in missing.Chunk(LookupBatchSize))
        {
            var users = await _client.GetNamesAsync(batch);

            foreach (var user in users)
            {
                if (string.IsNullOrWhiteSpace(user.DisplayName))
                {
                    continue;
                }

                result[user.Id] = user.DisplayName;
                _cache.Set(CacheKey(user.Id), user.DisplayName, CacheDuration);
            }
        }

        return result;
    }

    public async Task EnrichAsync(IEnumerable<TicketResponse> tickets)
    {
        var list = tickets.ToList();

        if (list.Count == 0)
        {
            return;
        }

        var ids = new List<Guid?>();

        foreach (var ticket in list)
        {
            ids.Add(ticket.CreatedByUserId);
            ids.Add(ticket.AssignedTechnicianId);

            ids.AddRange(ticket.Comments.Select(c => (Guid?)c.AuthorUserId));

            ids.AddRange(ticket.TicketHistories.Select(h =>
                Guid.TryParse(h.PerformedByUserId, out var performedBy) ? performedBy : (Guid?)null));
        }

        var names = await ResolveAsync(ids);

        foreach (var ticket in list)
        {
            ticket.CreatedByName = NameOf(names, ticket.CreatedByUserId);

            ticket.AssignedTechnicianName = ticket.AssignedTechnicianId.HasValue
                ? NameOf(names, ticket.AssignedTechnicianId.Value)
                : null;

            foreach (var comment in ticket.Comments)
            {
                comment.AuthorName = NameOf(names, comment.AuthorUserId);
            }

            foreach (var history in ticket.TicketHistories)
            {
                history.PerformedByName = Guid.TryParse(history.PerformedByUserId, out var performedBy)
                    ? NameOf(names, performedBy)
                    : SystemName;
            }
        }
    }

    public async Task EnrichAsync(IEnumerable<CommentResponse> comments)
    {
        var list = comments.ToList();

        if (list.Count == 0)
        {
            return;
        }

        var names = await ResolveAsync(list.Select(c => (Guid?)c.AuthorUserId));

        foreach (var comment in list)
        {
            comment.AuthorName = NameOf(names, comment.AuthorUserId);
        }
    }

    private static string NameOf(IReadOnlyDictionary<Guid, string> names, Guid id)
        => names.TryGetValue(id, out var name) ? name : UnknownUser;

    private static string CacheKey(Guid id) => $"user-name:{id}";
}
