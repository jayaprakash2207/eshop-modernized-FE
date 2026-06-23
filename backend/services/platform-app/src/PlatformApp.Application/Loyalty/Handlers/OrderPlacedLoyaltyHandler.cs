using MediatR;
using Microsoft.Extensions.Logging;
using PlatformApp.Application.Abstractions;
using PlatformApp.Application.Orders.Events;

namespace PlatformApp.Application.Loyalty.Handlers;

/// <summary>
/// Bridges the in-process OrderPlaced notification to the message bus. It publishes
/// an integration event that the bus consumer (OrderPlacedConsumer) handles to award
/// loyalty points. Keeping a SINGLE award path (the bus consumer) avoids
/// double-crediting; idempotency is still guaranteed by SourceEventId.
/// </summary>
public sealed class OrderPlacedLoyaltyHandler : INotificationHandler<OrderPlacedNotification>
{
    private readonly IIntegrationEventPublisher _bus;
    private readonly ILogger<OrderPlacedLoyaltyHandler> _logger;

    public OrderPlacedLoyaltyHandler(IIntegrationEventPublisher bus, ILogger<OrderPlacedLoyaltyHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(OrderPlacedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            await _bus.PublishOrderPlacedAsync(
                notification.OrderId,
                notification.BuyerId,
                notification.Total,
                notification.SourceEventId,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Bus publish failure must not roll back a completed order.
            _logger.LogError(ex,
                "Failed to publish OrderPlaced integration event for order {OrderId}.",
                notification.OrderId);
        }
    }
}
