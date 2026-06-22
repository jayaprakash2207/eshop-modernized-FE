using PlatformApp.Domain.Orders;
using PlatformApp.Application.Orders;
using PlatformApp.Infrastructure.State;

namespace PlatformApp.Infrastructure.Orders;

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly AppState _state;

    public InMemoryOrderRepository(AppState state)
    {
        _state = state;
    }

    public Task SaveAsync(Order order, CancellationToken cancellationToken)
    {
        _state.Orders.Add(order);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<Order>> ListByBuyerAsync(Guid buyerId, CancellationToken cancellationToken)
    {
        var orders = _state.Orders.Where(order => order.BuyerId == buyerId).ToArray();
        return Task.FromResult<IReadOnlyCollection<Order>>(orders);
    }

    public Task<Order?> GetByIdAsync(Guid buyerId, Guid orderId, CancellationToken cancellationToken)
    {
        var order = _state.Orders.SingleOrDefault(existing => existing.BuyerId == buyerId && existing.Id == orderId);
        return Task.FromResult(order);
    }
}
