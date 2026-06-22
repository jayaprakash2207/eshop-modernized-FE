using PlatformApp.Domain.Basket;

namespace PlatformApp.Application.Basket;

public interface IBasketRepository
{
    Task<Domain.Basket.Basket> GetOrCreateAsync(Guid buyerId, CancellationToken cancellationToken);
    Task SaveAsync(Domain.Basket.Basket basket, CancellationToken cancellationToken);
}
