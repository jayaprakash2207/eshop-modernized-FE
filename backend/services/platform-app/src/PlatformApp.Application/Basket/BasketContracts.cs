namespace PlatformApp.Application.Basket;

public sealed record BasketItemDto(Guid CatalogItemId, string ProductName, decimal UnitPrice, int Quantity, decimal Total);

public sealed record BasketDto(Guid BuyerId, IReadOnlyCollection<BasketItemDto> Items, decimal Total);

public sealed record AddBasketItemRequest(Guid CatalogItemId, int Quantity);

public sealed record UpdateBasketItemRequest(Guid CatalogItemId, int Quantity);

public sealed record CheckoutRequest(
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country);

public sealed record CheckoutResult(Guid OrderId, string OrderNumber, decimal Total, string RedirectUri);
