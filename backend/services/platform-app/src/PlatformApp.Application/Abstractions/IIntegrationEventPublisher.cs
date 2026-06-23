namespace PlatformApp.Application.Abstractions;

/// <summary>
/// Application-layer abstraction over the message bus. Keeps the Application layer
/// free of any transport dependency (MassTransit/RabbitMQ live in Infrastructure).
/// </summary>
public interface IIntegrationEventPublisher
{
    Task PublishOrderPlacedAsync(Guid orderId, Guid buyerId, decimal total, string sourceEventId, CancellationToken cancellationToken);
}
