namespace PlatformApp.Domain.Basket;

public sealed class BasketItem
{
    public BasketItem(Guid catalogItemId, string productName, decimal unitPrice, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        CatalogItemId = catalogItemId;
        ProductName = productName.Trim();
        UnitPrice = decimal.Round(unitPrice, 2, MidpointRounding.AwayFromZero);
        Quantity = quantity;
    }

    public Guid CatalogItemId { get; }
    public string ProductName { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal Total => UnitPrice * Quantity;

    public void SetQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        Quantity = quantity;
    }
}
