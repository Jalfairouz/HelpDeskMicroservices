using System.Diagnostics;
using System.Text.Json;
using IntegrationContracts.Events;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Forwarder;
namespace ApiGateway.Logging;

public static class ApiLogTransforms
{
    private const string CorrelationHeader = "X-Correlation-Id";
    private const string StateKey = "apilog.state";

    private static readonly string[] SkipPrefixes = ["/hubs", "/api/logs"];

    private static readonly HashSet<string> SensitiveQueryKeys =
        new(StringComparer.OrdinalIgnoreCase) { "access_token", "token", "password" };

    private sealed record LogState(string CorrelationId, DateTime StartedAt, long StartTimestamp);

    public static IReverseProxyBuilder AddApiLogging(this IReverseProxyBuilder proxy)
    {
        return proxy.AddTransforms(builder =>
        {
            var service = builder.Route.ClusterId;

            builder.AddRequestTransform(context =>
            {
                var http = context.HttpContext;
                var path = http.Request.Path.Value ?? "/";

                if (HttpMethods.IsOptions(http.Request.Method) ||
                    SkipPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                {
                    return ValueTask.CompletedTask;
                }

                var state = new LogState(
                    Guid.NewGuid().ToString("N"),
                    DateTime.UtcNow,
                    Stopwatch.GetTimestamp());

                http.Items[StateKey] = state;

                context.ProxyRequest.Headers.Remove(CorrelationHeader);
                context.ProxyRequest.Headers.TryAddWithoutValidation(CorrelationHeader, state.CorrelationId);

                var (userId, role) = ReadClaims(http);

                http.RequestServices.GetRequiredService<ApiLogQueue>().TryWrite(new ApiRequestLoggedEvent
                {
                    CorrelationId = state.CorrelationId,
                    Timestamp = state.StartedAt,
                    Method = http.Request.Method,
                    Path = path,
                    Query = Redact(http.Request.Query),
                    Service = service,
                    UserId = userId,
                    Role = role,
                    ClientIp = http.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = http.Request.Headers.UserAgent.ToString(),
                    RequestSize = http.Request.ContentLength
                });

                return ValueTask.CompletedTask;
            });

            builder.AddResponseTransform(context =>
            {
                var http = context.HttpContext;
                if (http.Items[StateKey] is not LogState state)
                    return ValueTask.CompletedTask;

                var status = context.ProxyResponse is { } response
                    ? (int)response.StatusCode
                    : http.Response.StatusCode;
                var forwarderError = http.Features.Get<IForwarderErrorFeature>()?.Error;

                http.Response.Headers[CorrelationHeader] = state.CorrelationId;

                http.RequestServices.GetRequiredService<ApiLogQueue>().TryWrite(new ApiResponseLoggedEvent
                {
                    CorrelationId = state.CorrelationId,
                    RequestTimestamp = state.StartedAt,
                    Timestamp = DateTime.UtcNow,
                    StatusCode = status,
                    Error = forwarderError is null or ForwarderError.None ? null : forwarderError.ToString(),
                    DurationMs = (long)Stopwatch.GetElapsedTime(state.StartTimestamp).TotalMilliseconds,
                    ResponseSize = context.ProxyResponse?.Content.Headers.ContentLength
                });

                return ValueTask.CompletedTask;
            });
        });
    }

    private static string? Redact(IQueryCollection query)
    {
        if (query.Count == 0) return null;

        return string.Join("&", query.Select(q =>
            SensitiveQueryKeys.Contains(q.Key) ? $"{q.Key}=***" : $"{q.Key}={q.Value}"));
    }

    private static (string? userId, string? role) ReadClaims(HttpContext context)
    {
        try
        {
            var header = context.Request.Headers.Authorization.ToString();
            var token = header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? header[7..]
                : (string?)context.Request.Query["access_token"];

            var parts = token?.Split('.');
            if (parts is not { Length: 3 }) return (null, null);

            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');

            using var doc = JsonDocument.Parse(Convert.FromBase64String(payload));
            var root = doc.RootElement;

            return (
                Read(root, "nameid", "sub",
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"),
                Read(root, "role",
                    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"));
        }
        catch
        {
            return (null, null);
        }
    }

    private static string? Read(JsonElement root, params string[] names)
    {
        foreach (var name in names)
        {
            if (root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
                return value.GetString();
        }

        return null;
    }
}