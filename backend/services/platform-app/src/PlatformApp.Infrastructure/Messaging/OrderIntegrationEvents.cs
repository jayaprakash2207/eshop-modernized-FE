namespace PlatformApp.Infrastructure.Messaging;

/// <summary>
/// Integration event published to the message bus when an order is placed.
/// Consumers (loyalty, shipping, notifications) react independently — the
/// choreography-based saga. Distinct from the in-process MediatR notification:
/// this one crosses service/process boundaries via RabbitMQ.
/// </summary>
public sealed record OrderPlacedIntegrationEvent
{
    public Guid OrderId { get; init; }
    public Guid BuyerId { get; init; }
    public decimal Total { get; init; }
    public string SourceEventId { get; init; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; init; }
}
