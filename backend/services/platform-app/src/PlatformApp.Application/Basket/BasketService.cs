using MediatR;
using PlatformApp.Application.Catalog;
using PlatformApp.Domain.Basket;
using PlatformApp.Domain.Orders;
using PlatformApp.Application.Orders;
using PlatformApp.Application.Orders.Events;

namespace PlatformApp.Application.Basket;

public sealed class BasketService
{
    private readonly IBasketRepository _basketRepository;
    private readonly ICatalogRepository _catalogRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IPublisher _publisher;

    public BasketService(
        IBasketRepository basketRepository,
        ICatalogRepository catalogRepository,
        IOrderRepository orderRepository,
        IPublisher publisher)
    {
        _basketRepository = basketRepository;
        _catalogRepository = catalogRepository;
        _orderRepository = orderRepository;
        _publisher = publisher;
    }

    public async Task<BasketDto> GetAsync(Guid buyerId, CancellationToken cancellationToken)
    {
        var basket = await _basketRepository.GetOrCreateAsync(buyerId, cancellationToken);
        return Map(basket);
    }

    public async Task<BasketDto> AddItemAsync(Guid buyerId, AddBasketItemRequest request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(request.Quantity));
        }

        if (request.Quantity > 999)
        {
            throw new ArgumentException("Quantity cannot exceed 999 per item.", nameof(request.Quantity));
        }

        var basket = await _basketRepository.GetOrCreateAsync(buyerId, cancellationToken);
        var item = await _catalogRepository.GetItemByIdAsync(request.CatalogItemId, cancellationToken)
            ?? throw new InvalidOperationException("Catalog item was not found.");

        if (item.AvailableStock < request.Quantity)
        {
            throw new InvalidOperationException($"Not enough stock available. Requested: {request.Quantity}, Available: {item.AvailableStock}");
        }

        basket.AddItem(item.Id, item.Name, item.Price, request.Quantity);
        await _basketRepository.SaveAsync(basket, cancellationToken);
        return Map(basket);
    }

    public async Task<BasketDto> UpdateItemAsync(Guid buyerId, UpdateBasketItemRequest request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(request.Quantity));
        }

        if (request.Quantity > 999)
        {
            throw new ArgumentException("Quantity cannot exceed 999 per item.", nameof(request.Quantity));
        }

        var basket = await _basketRepository.GetOrCreateAsync(buyerId, cancellationToken);
        var item = await _catalogRepository.GetItemByIdAsync(request.CatalogItemId, cancellationToken)
            ?? throw new InvalidOperationException("Catalog item was not found.");

        if (item.AvailableStock < request.Quantity)
        {
            throw new InvalidOperationException($"Not enough stock available. Requested: {request.Quantity}, Available: {item.AvailableStock}");
        }

        basket.UpdateItem(request.CatalogItemId, request.Quantity);
        await _basketRepository.SaveAsync(basket, cancellationToken);
        return Map(basket);
    }

    public async Task<BasketDto> RemoveItemAsync(Guid buyerId, Guid catalogItemId, CancellationToken cancellationToken)
    {
        var basket = await _basketRepository.GetOrCreateAsync(buyerId, cancellationToken);
        basket.RemoveItem(catalogItemId);
        await _basketRepository.SaveAsync(basket, cancellationToken);
        return Map(basket);
    }

    public async Task<CheckoutResult> CheckoutAsync(Guid buyerId, string buyerUsername, CheckoutRequest request, CancellationToken cancellationToken)
    {
        var basket = await _basketRepository.GetOrCreateAsync(buyerId, cancellationToken);
        if (!basket.Items.Any())
        {
            throw new InvalidOperationException("Basket is empty.");
        }

        var order = new Order(
            buyerId,
            buyerUsername,
            new Address(request.Street, request.City, request.State, request.PostalCode, request.Country),
            basket.Items.Select(item => new OrderItem(item.CatalogItemId, item.ProductName, item.UnitPrice, item.Quantity)));

        await _orderRepository.SaveAsync(order, cancellationToken);
        basket.Clear();
        await _basketRepository.SaveAsync(basket, cancellationToken);

        // Raise OrderPlaced — the Loyalty context consumes this to award points.
        // SourceEventId = order id makes the downstream earn idempotent.
        await _publisher.Publish(
            new OrderPlacedNotification(order.Id, buyerId, order.Total, order.Id.ToString()),
            cancellationToken);

        return new CheckoutResult(order.Id, order.OrderNumber, order.Total, "/Basket/Success");
    }

    private static BasketDto Map(Domain.Basket.Basket basket)
    {
        return new BasketDto(
            basket.BuyerId,
            basket.Items.Select(item => new BasketItemDto(item.CatalogItemId, item.ProductName, item.UnitPrice, item.Quantity, item.Total)).ToArray(),
            basket.Total);
    }
}
