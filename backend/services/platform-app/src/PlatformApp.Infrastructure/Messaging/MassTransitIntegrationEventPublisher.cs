using MassTransit;
using PlatformApp.Application.Abstractions;

namespace PlatformApp.Infrastructure.Messaging;

/// <summary>
/// MassTransit-backed implementation of <see cref="IIntegrationEventPublisher"/>.
/// Publishes integration events onto the configured transport (RabbitMQ or in-memory).
/// </summary>
public sealed class MassTransitIntegrationEventPublisher : IIntegrationEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitIntegrationEventPublisher(IPublishEndpoint publishEndpoint)
        => _publishEndpoint = publishEndpoint;

    public Task PublishOrderPlacedAsync(Guid orderId, Guid buyerId, decimal total, string sourceEventId, CancellationToken cancellationToken)
        => _publishEndpoint.Publish(new OrderPlacedIntegrationEvent
        {
            OrderId       = orderId,
            BuyerId       = buyerId,
            Total         = total,
            SourceEventId = sourceEventId,
            OccurredAtUtc = DateTimeOffset.UtcNow
        }, cancellationToken);
}
