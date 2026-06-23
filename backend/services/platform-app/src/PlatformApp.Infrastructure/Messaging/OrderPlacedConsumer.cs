using MassTransit;
using Microsoft.Extensions.Logging;
using PlatformApp.Application.Loyalty;

namespace PlatformApp.Infrastructure.Messaging;

/// <summary>
/// Saga step: consumes <see cref="OrderPlacedIntegrationEvent"/> off the bus and
/// awards loyalty points. Idempotent via SourceEventId. This is the distributed
/// (cross-service) counterpart to the in-process MediatR loyalty handler — in a
/// fully extracted microservice topology this consumer would live in the Loyalty
/// service and the MediatR handler would be removed.
/// </summary>
public sealed class OrderPlacedConsumer : IConsumer<OrderPlacedIntegrationEvent>
{
    private readonly ILoyaltyService _loyaltyService;
    private readonly ILogger<OrderPlacedConsumer> _logger;

    public OrderPlacedConsumer(ILoyaltyService loyaltyService, ILogger<OrderPlacedConsumer> logger)
    {
        _loyaltyService = loyaltyService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlacedIntegrationEvent> context)
    {
        var msg = context.Message;
        var points = await _loyaltyService.EarnPointsAsync(
            msg.BuyerId,
            new EarnPointsRequest(msg.OrderId, msg.Total, msg.SourceEventId),
            context.CancellationToken);

        _logger.LogInformation(
            "[saga] Awarded {Points} points for order {OrderId} via message bus", points, msg.OrderId);
    }
}
