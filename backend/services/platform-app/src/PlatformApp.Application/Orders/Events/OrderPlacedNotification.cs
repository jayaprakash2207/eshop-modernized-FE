using MediatR;

namespace PlatformApp.Application.Orders.Events;

/// <summary>
/// Integration event raised when an order is successfully placed at checkout.
/// Consumed by the Loyalty context to award points — keeps Basket/Order decoupled
/// from Loyalty (KG: "integrates with OrderContext via OrderPlaced events").
/// </summary>
public sealed record OrderPlacedNotification(
    Guid OrderId,
    Guid BuyerId,
    decimal Total,
    string SourceEventId) : INotification;
