using PlatformApp.Application.Catalog;
using PlatformApp.Domain.Catalog;
using PlatformApp.Infrastructure.State;

namespace PlatformApp.Infrastructure.Catalog;

public sealed class InMemoryCatalogRepository : ICatalogRepository
{
    private readonly AppState _state;

    public InMemoryCatalogRepository(AppState state)
    {
        _state = state;
    }

    public Task<IReadOnlyCollection<CatalogBrand>> ListBrandsAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyCollection<CatalogBrand>>(_state.Brands.OrderBy(brand => brand.Name).ToArray());

    public Task<IReadOnlyCollection<CatalogType>> ListTypesAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyCollection<CatalogType>>(_state.Types.OrderBy(type => type.Name).ToArray());

    public Task<(IReadOnlyCollection<CatalogItem> Items, int TotalCount)> ListItemsAsync(CatalogItemsQuery query, CancellationToken cancellationToken)
    {
        IEnumerable<CatalogItem> filtered = _state.CatalogItems;

        if (query.CatalogBrandId.HasValue)
        {
            filtered = filtered.Where(item => item.CatalogBrandId == query.CatalogBrandId.Value);
        }

        if (query.CatalogTypeId.HasValue)
        {
            filtered = filtered.Where(item => item.CatalogTypeId == query.CatalogTypeId.Value);
        }

        var totalCount = filtered.Count();
        var items = filtered
            .OrderBy(item => item.Name)
            .Skip(query.PageIndex * query.PageSize)
            .Take(query.PageSize)
            .ToArray();

        return Task.FromResult(((IReadOnlyCollection<CatalogItem>)items, totalCount));
    }

    public Task<CatalogItem?> GetItemByIdAsync(Guid catalogItemId, CancellationToken cancellationToken) =>
        Task.FromResult(_state.CatalogItems.SingleOrDefault(item => item.Id == catalogItemId));

    public Task<CatalogItem> AddItemAsync(CatalogItem item, CancellationToken cancellationToken)
    {
        _state.CatalogItems.Add(item);
        return Task.FromResult(item);
    }

    public Task<CatalogItem> UpdateItemAsync(CatalogItem item, CancellationToken cancellationToken) =>
        Task.FromResult(item);

    public Task<bool> DeleteItemAsync(Guid catalogItemId, CancellationToken cancellationToken)
    {
        var item = _state.CatalogItems.SingleOrDefault(existing => existing.Id == catalogItemId);
        if (item is null)
        {
            return Task.FromResult(false);
        }

        _state.CatalogItems.Remove(item);
        return Task.FromResult(true);
    }
}
