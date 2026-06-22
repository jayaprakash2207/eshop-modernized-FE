using PlatformApp.Domain.Orders;

namespace PlatformApp.Application.Orders;

public interface IOrderRepository
{
    Task SaveAsync(Order order, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Order>> ListByBuyerAsync(Guid buyerId, CancellationToken cancellationToken);
    Task<Order?> GetByIdAsync(Guid buyerId, Guid orderId, CancellationToken cancellationToken);
}
