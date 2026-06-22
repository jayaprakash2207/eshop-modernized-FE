using PlatformApp.Application.Basket;
using PlatformApp.Infrastructure.State;

namespace PlatformApp.Infrastructure.Basket;

public sealed class InMemoryBasketRepository : IBasketRepository
{
    private readonly AppState _state;

    public InMemoryBasketRepository(AppState state)
    {
        _state = state;
    }

    public Task<Domain.Basket.Basket> GetOrCreateAsync(Guid buyerId, CancellationToken cancellationToken)
    {
        if (!_state.Baskets.TryGetValue(buyerId, out var basket))
        {
            basket = new Domain.Basket.Basket(buyerId);
            _state.Baskets[buyerId] = basket;
        }

        return Task.FromResult(basket);
    }

    public Task SaveAsync(Domain.Basket.Basket basket, CancellationToken cancellationToken)
    {
        _state.Baskets[basket.BuyerId] = basket;
        return Task.CompletedTask;
    }
}
