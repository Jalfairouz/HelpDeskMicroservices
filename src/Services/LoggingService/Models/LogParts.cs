namespace LoggingService.Models;

public sealed record RequestPart(
    string CorrelationId,
    DateTime Timestamp,
    string Method,
    string Path,
    string? Query,
    string? Service,
    string? UserId,
    string? Role,
    string? ClientIp,
    string? UserAgent,
    long? RequestSize);

public sealed record ResponsePart(
    string CorrelationId,
    DateTime RespondedAt,
    int StatusCode,
    string? Error,
    long DurationMs,
    long? ResponseSize);