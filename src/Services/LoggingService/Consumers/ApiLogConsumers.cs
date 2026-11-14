using IntegrationContracts.Events;
using LoggingService.Services;
using MassTransit;

namespace LoggingService.Consumers;

public sealed class ApiRequestBatchConsumer : IConsumer<Batch<ApiRequestLoggedEvent>>
{
    private readonly IApiLogStore _store;

    public ApiRequestBatchConsumer(IApiLogStore store) => _store = store;

    public Task Consume(ConsumeContext<Batch<ApiRequestLoggedEvent>> context)
        => _store.SaveRequestsAsync(context.Message.Select(m => m.Message), context.CancellationToken);
}

public sealed class ApiResponseBatchConsumer : IConsumer<Batch<ApiResponseLoggedEvent>>
{
    private readonly IApiLogStore _store;

    public ApiResponseBatchConsumer(IApiLogStore store) => _store = store;

    public Task Consume(ConsumeContext<Batch<ApiResponseLoggedEvent>> context)
        => _store.SaveResponsesAsync(context.Message.Select(m => m.Message), context.CancellationToken);
}

public sealed class ApiRequestBatchConsumerDefinition : ConsumerDefinition<ApiRequestBatchConsumer>
{
    public ApiRequestBatchConsumerDefinition() => EndpointName = "logging.api-requests";

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpoint,
        IConsumerConfigurator<ApiRequestBatchConsumer> consumer,
        IRegistrationContext context)
    {
        endpoint.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)));

        consumer.Options<BatchOptions>(o => o
            .SetMessageLimit(100)
            .SetTimeLimit(TimeSpan.FromSeconds(3))
            .SetConcurrencyLimit(2));
    }
}

public sealed class ApiResponseBatchConsumerDefinition : ConsumerDefinition<ApiResponseBatchConsumer>
{
    public ApiResponseBatchConsumerDefinition() => EndpointName = "logging.api-responses";

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpoint,
        IConsumerConfigurator<ApiResponseBatchConsumer> consumer,
        IRegistrationContext context)
    {
        endpoint.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)));

        consumer.Options<BatchOptions>(o => o
            .SetMessageLimit(100)
            .SetTimeLimit(TimeSpan.FromSeconds(3))
            .SetConcurrencyLimit(2));
    }
}