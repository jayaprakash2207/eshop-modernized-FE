using PlatformApp.Domain.Orders;

namespace PlatformApp.Application.Orders;

public sealed class OrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IReadOnlyCollection<OrderSummaryDto>> GetMyOrdersAsync(Guid buyerId, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.ListByBuyerAsync(buyerId, cancellationToken);
        return orders
            .OrderByDescending(order => order.CreatedAtUtc)
            .Select(order => new OrderSummaryDto(order.Id, order.OrderNumber, order.Status, order.Total, order.CreatedAtUtc))
            .ToArray();
    }

    public async Task<OrderDetailDto?> GetOrderDetailAsync(Guid buyerId, Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(buyerId, orderId, cancellationToken);
        return order is null ? null : Map(order);
    }

    private static OrderDetailDto Map(Order order)
    {
        return new OrderDetailDto(
            order.Id,
            order.OrderNumber,
            order.Status,
            order.Total,
            order.CreatedAtUtc,
            new AddressDto(
                order.ShippingAddress.Street,
                order.ShippingAddress.City,
                order.ShippingAddress.State,
                order.ShippingAddress.PostalCode,
                order.ShippingAddress.Country),
            order.Items.Select(item => new OrderItemDto(item.CatalogItemId, item.ProductName, item.UnitPrice, item.Units, item.LineTotal)).ToArray());
    }
}
