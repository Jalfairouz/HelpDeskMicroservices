using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using IntegrationContracts.Events;
using LoggingService.Models;

namespace LoggingService.Services;

public interface IApiLogStore
{
    Task EnsureTemplateAsync(CancellationToken ct);
    Task SaveRequestsAsync(IEnumerable<ApiRequestLoggedEvent> events, CancellationToken ct);
    Task SaveResponsesAsync(IEnumerable<ApiResponseLoggedEvent> events, CancellationToken ct);
    Task<LogsPage> GetLogsAsync(int page, int size, CancellationToken ct);
}

public sealed class ElasticLogStore : IApiLogStore
{
    private const string IndexPrefix = "helpdesk-api-logs-";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _http;

    public ElasticLogStore(HttpClient http) => _http = http;

    public async Task EnsureTemplateAsync(CancellationToken ct)
    {
        const string template = """
        {
          "index_patterns": ["helpdesk-api-logs-*"],
          "template": {
            "settings": { "number_of_shards": 1, "number_of_replicas": 0 },
            "mappings": {
              "properties": {
                "correlationId": { "type": "keyword" },
                "timestamp":     { "type": "date" },
                "respondedAt":   { "type": "date" },
                "method":        { "type": "keyword" },
                "path":          { "type": "keyword" },
                "query":         { "type": "keyword", "ignore_above": 512 },
                "service":       { "type": "keyword" },
                "userId":        { "type": "keyword" },
                "role":          { "type": "keyword" },
                "clientIp":      { "type": "keyword" },
                "userAgent":     { "type": "keyword", "ignore_above": 256 },
                "requestSize":   { "type": "long" },
                "statusCode":    { "type": "integer" },
                "error": { "type": "keyword" },
                "durationMs":    { "type": "long" },
                "responseSize":  { "type": "long" }
              }
            }
          }
        }
        """;

        using var content = new StringContent(template, Encoding.UTF8, "application/json");
        var response = await _http.PutAsync("_index_template/helpdesk-api-logs", content, ct);
        response.EnsureSuccessStatusCode();
    }

    public Task SaveRequestsAsync(IEnumerable<ApiRequestLoggedEvent> events, CancellationToken ct)
    {
        var items = events
            .Where(e => !string.IsNullOrWhiteSpace(e.CorrelationId))
            .Select(e => (
                Index: IndexFor(e.Timestamp),
                Id: e.CorrelationId,
                Doc: (object)new RequestPart(
                    e.CorrelationId, e.Timestamp.ToUniversalTime(), e.Method, e.Path, e.Query,
                    e.Service, e.UserId, e.Role, e.ClientIp, e.UserAgent, e.RequestSize)))
            .ToList();

        return BulkUpsertAsync(items, ct);
    }

    public Task SaveResponsesAsync(IEnumerable<ApiResponseLoggedEvent> events, CancellationToken ct)
    {
        var items = events
            .Where(e => !string.IsNullOrWhiteSpace(e.CorrelationId))
            .Select(e => (
                Index: IndexFor(e.RequestTimestamp),
                Id: e.CorrelationId,
                Doc: (object)new ResponsePart(
                    e.CorrelationId, e.Timestamp.ToUniversalTime(), e.StatusCode, e.Error, e.DurationMs, e.ResponseSize)))
            .ToList();

        return BulkUpsertAsync(items, ct);
    }
    public async Task<LogsPage> GetLogsAsync(int page, int size, CancellationToken ct)
    {
        var request = new
        {
            from = (page - 1) * size,
            size,
            track_total_hits = true,
            sort = new[] { new { timestamp = new { order = "desc", unmapped_type = "date" } } },
            query = new { match_all = new { } }
        };

        using var content = new StringContent(
            JsonSerializer.Serialize(request, Json), Encoding.UTF8, "application/json");

        var response = await _http.PostAsync($"{IndexPrefix}*/_search", content, ct);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var hits = doc.RootElement.GetProperty("hits");

        var items = hits.GetProperty("hits").EnumerateArray()
            .Select(h => h.GetProperty("_source").Deserialize<ApiLogDocument>(Json)!)
            .ToList();

        return new LogsPage(
            hits.GetProperty("total").GetProperty("value").GetInt64(), page, size, items);
    }
    private static string IndexFor(DateTime timestamp)
        => $"{IndexPrefix}{timestamp.ToUniversalTime():yyyy.MM.dd}";

    private async Task BulkUpsertAsync(
        IReadOnlyCollection<(string Index, string Id, object Doc)> items, CancellationToken ct)
    {
        if (items.Count == 0) return;

        var body = new StringBuilder();

        foreach (var (index, id, doc) in items)
        {
            body.Append(JsonSerializer.Serialize(
                new { update = new { _index = index, _id = id, retry_on_conflict = 5 } }, Json)).Append('\n');

            body.Append(JsonSerializer.Serialize(
                new { doc, doc_as_upsert = true }, Json)).Append('\n');
        }

        using var content = new StringContent(body.ToString(), Encoding.UTF8, "application/x-ndjson");
        var response = await _http.PostAsync("_bulk", content, ct);
        response.EnsureSuccessStatusCode();

        using var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));

        if (result.RootElement.GetProperty("errors").GetBoolean())
        {
            var failed = result.RootElement.GetProperty("items").EnumerateArray()
                .Select(item => item.EnumerateObject().First().Value)
                .FirstOrDefault(v => v.TryGetProperty("error", out _));

            throw new InvalidOperationException($"Elasticsearch bulk update failed: {failed}");
        }
    }
}

public sealed class ElasticBootstrapper : BackgroundService
{
    private readonly IApiLogStore _store;
    private readonly ILogger<ElasticBootstrapper> _logger;

    public ElasticBootstrapper(IApiLogStore store, ILogger<ElasticBootstrapper> logger)
    {
        _store = store;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _store.EnsureTemplateAsync(stoppingToken);
                _logger.LogInformation("Elasticsearch index template is ready.");
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning("Elasticsearch not ready ({Message}). Retrying in 5s...", ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}