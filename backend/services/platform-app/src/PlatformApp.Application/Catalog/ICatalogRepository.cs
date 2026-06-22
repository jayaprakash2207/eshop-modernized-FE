using PlatformApp.Domain.Catalog;

namespace PlatformApp.Application.Catalog;

public interface ICatalogRepository
{
    Task<IReadOnlyCollection<CatalogBrand>> ListBrandsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CatalogType>> ListTypesAsync(CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<CatalogItem> Items, int TotalCount)> ListItemsAsync(CatalogItemsQuery query, CancellationToken cancellationToken);
    Task<CatalogItem?> GetItemByIdAsync(Guid catalogItemId, CancellationToken cancellationToken);
    Task<CatalogItem> AddItemAsync(CatalogItem item, CancellationToken cancellationToken);
    Task<CatalogItem> UpdateItemAsync(CatalogItem item, CancellationToken cancellationToken);
    Task<bool> DeleteItemAsync(Guid catalogItemId, CancellationToken cancellationToken);
}
