using System.Threading.Channels;
using MassTransit;

namespace ApiGateway.Logging;

public sealed class ApiLogQueue
{
    private readonly Channel<object> _channel = Channel.CreateBounded<object>(
        new BoundedChannelOptions(10_000) { FullMode = BoundedChannelFullMode.DropOldest });

    public bool TryWrite(object logEvent) => _channel.Writer.TryWrite(logEvent);

    public IAsyncEnumerable<object> ReadAllAsync(CancellationToken ct) => _channel.Reader.ReadAllAsync(ct);
}

public sealed class ApiLogPublisher : BackgroundService
{
    private readonly ApiLogQueue _queue;
    private readonly IBus _bus;
    private readonly ILogger<ApiLogPublisher> _logger;

    public ApiLogPublisher(ApiLogQueue queue, IBus bus, ILogger<ApiLogPublisher> logger)
    {
        _queue = queue;
        _bus = bus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var logEvent in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await _bus.Publish(logEvent, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Failed to publish API log event.");
            }
        }
    }
}