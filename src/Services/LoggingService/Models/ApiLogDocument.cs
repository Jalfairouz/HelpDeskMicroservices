namespace LoggingService.Models;

public sealed class ApiLogDocument
{
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime? Timestamp { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string? Method { get; set; }
    public string? Path { get; set; }
    public string? Query { get; set; }
    public string? Service { get; set; }
    public string? UserId { get; set; }
    public string? Role { get; set; }
    public string? ClientIp { get; set; }
    public string? UserAgent { get; set; }
    public long? RequestSize { get; set; }
    public int? StatusCode { get; set; }
    public long? DurationMs { get; set; }
    public long? ResponseSize { get; set; }
    public string? Error { get; set; }
}

public sealed record LogsPage(long Total, int Page, int Size, IReadOnlyList<ApiLogDocument> Items);