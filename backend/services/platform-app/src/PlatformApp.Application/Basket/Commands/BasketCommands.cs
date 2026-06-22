using MediatR;

namespace PlatformApp.Application.Basket.Commands;

public record AddBasketItemCommand(Guid UserId, Guid CatalogItemId, int Quantity) : IRequest<BasketResponse>;

public record UpdateBasketItemCommand(Guid UserId, Guid CatalogItemId, int Quantity) : IRequest<BasketResponse>;

public record RemoveBasketItemCommand(Guid UserId, Guid CatalogItemId) : IRequest<BasketResponse>;

public record CheckoutBasketCommand(Guid UserId, string Username, string ShipToAddress, string City, string State, string ZipCode, string Country)
    : IRequest<CheckoutResult>;
