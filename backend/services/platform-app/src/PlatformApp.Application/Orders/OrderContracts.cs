namespace PlatformApp.Application.Orders;

public sealed record OrderItemDto(Guid CatalogItemId, string ProductName, decimal UnitPrice, int Units, decimal LineTotal);

public sealed record AddressDto(string Street, string City, string State, string PostalCode, string Country);

public sealed record OrderSummaryDto(Guid Id, string OrderNumber, string Status, decimal Total, DateTimeOffset CreatedAtUtc);

public sealed record OrderDetailDto(
    Guid Id,
    string OrderNumber,
    string Status,
    decimal Total,
    DateTimeOffset CreatedAtUtc,
    AddressDto ShippingAddress,
    IReadOnlyCollection<OrderItemDto> Items);
