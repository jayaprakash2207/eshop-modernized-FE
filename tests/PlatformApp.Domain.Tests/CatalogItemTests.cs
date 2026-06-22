using PlatformApp.Domain.Catalog;

namespace PlatformApp.Domain.Tests;

public class CatalogItemTests
{
    [Fact]
    public void Constructor_InitializesAggregateAndRaisesCreatedEvent()
    {
        var brandId = Guid.NewGuid();
        var typeId = Guid.NewGuid();

        var item = new CatalogItem(
            "Trail Jacket",
            "Water-resistant technical jacket.",
            129.00m,
            brandId,
            typeId,
            "/images/trail-jacket.png",
            12);

        Assert.Equal("Trail Jacket", item.Name);
        Assert.Single(item.DomainEvents);
    }

    [Fact]
    public void RemoveStock_ReturnsFalse_WhenQuantityExceedsAvailableStock()
    {
        var item = new CatalogItem(
            "Summit Bottle",
            "Insulated bottle.",
            34.50m,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "/images/summit-bottle.png",
            2);

        var removed = item.RemoveStock(3);

        Assert.False(removed);
        Assert.Equal(2, item.AvailableStock);
    }
}
