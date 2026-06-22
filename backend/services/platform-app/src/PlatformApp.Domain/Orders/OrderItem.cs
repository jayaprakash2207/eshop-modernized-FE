namespace PlatformApp.Domain.Orders;

public sealed class OrderItem
{
    public OrderItem(Guid catalogItemId, string productName, decimal unitPrice, int units)
    {
        CatalogItemId = catalogItemId;
        ProductName = productName.Trim();
        UnitPrice = decimal.Round(unitPrice, 2, MidpointRounding.AwayFromZero);
        Units = units;
    }

    public Guid CatalogItemId { get; }
    public string ProductName { get; }
    public decimal UnitPrice { get; }
    public int Units { get; }
    public decimal LineTotal => UnitPrice * Units;
}
