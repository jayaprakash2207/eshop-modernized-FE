using PlatformApp.Domain.Common;

namespace PlatformApp.Domain.Basket;

public sealed class Basket : Entity
{
    private readonly List<BasketItem> _items = [];

    public Basket(Guid buyerId)
    {
        BuyerId = buyerId;
    }

    public Guid BuyerId { get; private set; }
    public IReadOnlyCollection<BasketItem> Items => _items.AsReadOnly();
    public decimal Total => _items.Sum(item => item.Total);

    public void AddItem(Guid catalogItemId, string productName, decimal unitPrice, int quantity)
    {
        var existing = _items.SingleOrDefault(item => item.CatalogItemId == catalogItemId);
        if (existing is null)
        {
            _items.Add(new BasketItem(catalogItemId, productName, unitPrice, quantity));
        }
        else
        {
            existing.SetQuantity(existing.Quantity + quantity);
        }

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void UpdateItem(Guid catalogItemId, int quantity)
    {
        var existing = _items.SingleOrDefault(item => item.CatalogItemId == catalogItemId);
        if (existing is null)
        {
            throw new InvalidOperationException("Basket item was not found.");
        }

        existing.SetQuantity(quantity);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void RemoveItem(Guid catalogItemId)
    {
        var existing = _items.SingleOrDefault(item => item.CatalogItemId == catalogItemId);
        if (existing is null)
        {
            return;
        }

        _items.Remove(existing);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Clear()
    {
        _items.Clear();
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
