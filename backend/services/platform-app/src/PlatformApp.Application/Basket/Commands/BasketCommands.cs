using MediatR;

namespace PlatformApp.Application.Basket.Commands;

public record AddBasketItemCommand(Guid BuyerId, Guid CatalogItemId, int Quantity) : IRequest<BasketDto>;

public record UpdateBasketItemCommand(Guid BuyerId, Guid CatalogItemId, int Quantity) : IRequest<BasketDto>;

public record RemoveBasketItemCommand(Guid BuyerId, Guid CatalogItemId) : IRequest<BasketDto>;

public record CheckoutBasketCommand(
    Guid BuyerId,
    string Username,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country) : IRequest<CheckoutResult>;
