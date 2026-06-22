using PlatformApp.Domain.Common;

namespace PlatformApp.Domain.Catalog;

public sealed class CatalogItem : Entity
{
    private CatalogItem()
    {
    }

    public CatalogItem(
        string name,
        string description,
        decimal price,
        Guid catalogBrandId,
        Guid catalogTypeId,
        string pictureUri,
        int availableStock)
    {
        Name = ValidateName(name);
        Description = ValidateDescription(description);
        Price = ValidatePrice(price);
        CatalogBrandId = catalogBrandId;
        CatalogTypeId = catalogTypeId;
        PictureUri = pictureUri.Trim();
        AvailableStock = ValidateStock(availableStock);

        AddDomainEvent(new CatalogItemCreatedDomainEvent(Id, Name, DateTimeOffset.UtcNow));
    }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public Guid CatalogBrandId { get; private set; }
    public Guid CatalogTypeId { get; private set; }
    public string PictureUri { get; private set; } = string.Empty;
    public int AvailableStock { get; private set; }

    public static CatalogItem Restore(
        Guid id,
        string name,
        string description,
        decimal price,
        Guid catalogBrandId,
        Guid catalogTypeId,
        string pictureUri,
        int availableStock)
    {
        var item = new CatalogItem(name, description, price, catalogBrandId, catalogTypeId, pictureUri, availableStock);
        item.RestoreIdentity(id, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        item.ClearDomainEvents();
        return item;
    }

    public void UpdateDetails(
        string name,
        string description,
        decimal price,
        Guid catalogBrandId,
        Guid catalogTypeId,
        string pictureUri)
    {
        Name = ValidateName(name);
        Description = ValidateDescription(description);
        Price = ValidatePrice(price);
        CatalogBrandId = catalogBrandId;
        CatalogTypeId = catalogTypeId;
        PictureUri = pictureUri.Trim();
        UpdatedAtUtc = DateTimeOffset.UtcNow;

        AddDomainEvent(new CatalogItemUpdatedDomainEvent(Id, Name, DateTimeOffset.UtcNow));
    }

    public bool RemoveStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Stock quantity must be greater than zero.", nameof(quantity));
        }

        if (AvailableStock < quantity)
        {
            return false;
        }

        AvailableStock -= quantity;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        return true;
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Stock quantity must be greater than zero.", nameof(quantity));
        }

        AvailableStock += quantity;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Catalog item name is required.", nameof(name));
        }

        if (name.Length > 100)
        {
            throw new ArgumentException("Catalog item name cannot exceed 100 characters.", nameof(name));
        }

        return name.Trim();
    }

    private static string ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Catalog item description is required.", nameof(description));
        }

        return description.Trim();
    }

    private static decimal ValidatePrice(decimal price)
    {
        if (price < 0)
        {
            throw new ArgumentException("Catalog item price must be zero or greater.", nameof(price));
        }

        return decimal.Round(price, 2, MidpointRounding.AwayFromZero);
    }

    private static int ValidateStock(int availableStock)
    {
        if (availableStock < 0)
        {
            throw new ArgumentException("Available stock cannot be negative.", nameof(availableStock));
        }

        return availableStock;
    }
}
