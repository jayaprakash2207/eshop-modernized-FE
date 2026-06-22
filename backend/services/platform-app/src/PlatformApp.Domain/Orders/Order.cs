using PlatformApp.Domain.Common;

namespace PlatformApp.Domain.Orders;

public sealed class Order : Entity
{
    private readonly List<OrderItem> _items = [];

    public Order(Guid buyerId, string buyerUsername, Address shippingAddress, IEnumerable<OrderItem> items)
    {
        BuyerId = buyerId;
        BuyerUsername = buyerUsername.Trim();
        ShippingAddress = shippingAddress;
        OrderNumber = $"ESH-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        _items.AddRange(items);
        Status = "Submitted";
    }

    public Guid BuyerId { get; }
    public string BuyerUsername { get; }
    public string OrderNumber { get; private set; }
    public string Status { get; private set; }
    public Address ShippingAddress { get; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public decimal Total => _items.Sum(item => item.LineTotal);

    public static Order Restore(
        Guid id,
        Guid buyerId,
        string buyerUsername,
        string orderNumber,
        string status,
        Address shippingAddress,
        IEnumerable<OrderItem> items,
        DateTimeOffset createdAtUtc)
    {
        var order = new Order(buyerId, buyerUsername, shippingAddress, items)
        {
            OrderNumber = orderNumber,
            Status = status
        };

        order.RestoreIdentity(id, createdAtUtc, createdAtUtc);
        order.ClearDomainEvents();
        return order;
    }
}
