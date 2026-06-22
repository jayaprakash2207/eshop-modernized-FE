using PlatformApp.Domain.Catalog;

namespace PlatformApp.Application.Catalog;

public sealed class CatalogService
{
    private readonly ICatalogRepository _catalogRepository;

    public CatalogService(ICatalogRepository catalogRepository)
    {
        _catalogRepository = catalogRepository;
    }

    public async Task<IReadOnlyCollection<CatalogBrandDto>> GetBrandsAsync(CancellationToken cancellationToken)
    {
        var brands = await _catalogRepository.ListBrandsAsync(cancellationToken);
        return brands
            .Select(brand => new CatalogBrandDto(brand.Id, brand.Name))
            .OrderBy(brand => brand.Name)
            .ToArray();
    }

    public async Task<IReadOnlyCollection<CatalogTypeDto>> GetTypesAsync(CancellationToken cancellationToken)
    {
        var types = await _catalogRepository.ListTypesAsync(cancellationToken);
        return types
            .Select(type => new CatalogTypeDto(type.Id, type.Name))
            .OrderBy(type => type.Name)
            .ToArray();
    }

    public async Task<CatalogItemsResponse> GetItemsAsync(CatalogItemsQuery query, CancellationToken cancellationToken)
    {
        ValidateQuery(query);

        var (items, totalCount) = await _catalogRepository.ListItemsAsync(query, cancellationToken);
        var brands = await _catalogRepository.ListBrandsAsync(cancellationToken);
        var types = await _catalogRepository.ListTypesAsync(cancellationToken);

        var brandMap = brands.ToDictionary(brand => brand.Id, brand => brand.Name);
        var typeMap = types.ToDictionary(type => type.Id, type => type.Name);

        var mapped = items
            .Select(item => MapItem(item, brandMap, typeMap))
            .ToArray();

        return new CatalogItemsResponse(query.PageIndex, query.PageSize, totalCount, mapped);
    }

    public async Task<CatalogItemDto?> GetItemAsync(Guid catalogItemId, CancellationToken cancellationToken)
    {
        var item = await _catalogRepository.GetItemByIdAsync(catalogItemId, cancellationToken);
        if (item is null)
        {
            return null;
        }

        var brands = await _catalogRepository.ListBrandsAsync(cancellationToken);
        var types = await _catalogRepository.ListTypesAsync(cancellationToken);
        return MapItem(item, brands.ToDictionary(brand => brand.Id, brand => brand.Name), types.ToDictionary(type => type.Id, type => type.Name));
    }

    public Task<CatalogItem> CreateItemAsync(UpsertCatalogItemRequest request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        var item = new CatalogItem(
            request.Name,
            request.Description,
            request.Price,
            request.CatalogBrandId,
            request.CatalogTypeId,
            request.PictureUri,
            request.AvailableStock);

        return _catalogRepository.AddItemAsync(item, cancellationToken);
    }

    public async Task<CatalogItem?> UpdateItemAsync(Guid catalogItemId, UpsertCatalogItemRequest request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        var item = await _catalogRepository.GetItemByIdAsync(catalogItemId, cancellationToken);
        if (item is null)
        {
            return null;
        }

        item.UpdateDetails(
            request.Name,
            request.Description,
            request.Price,
            request.CatalogBrandId,
            request.CatalogTypeId,
            request.PictureUri);

        if (item.AvailableStock != request.AvailableStock)
        {
            if (request.AvailableStock > item.AvailableStock)
            {
                item.AddStock(request.AvailableStock - item.AvailableStock);
            }
            else
            {
                item.RemoveStock(item.AvailableStock - request.AvailableStock);
            }
        }

        return await _catalogRepository.UpdateItemAsync(item, cancellationToken);
    }

    public Task<bool> DeleteItemAsync(Guid catalogItemId, CancellationToken cancellationToken) =>
        _catalogRepository.DeleteItemAsync(catalogItemId, cancellationToken);

    private static CatalogItemDto MapItem(
        CatalogItem item,
        IReadOnlyDictionary<Guid, string> brandMap,
        IReadOnlyDictionary<Guid, string> typeMap)
    {
        return new CatalogItemDto(
            item.Id,
            item.Name,
            item.Description,
            item.Price,
            item.CatalogBrandId,
            brandMap.GetValueOrDefault(item.CatalogBrandId, "Unknown"),
            item.CatalogTypeId,
            typeMap.GetValueOrDefault(item.CatalogTypeId, "Unknown"),
            item.PictureUri,
            item.AvailableStock);
    }

    private static void ValidateQuery(CatalogItemsQuery query)
    {
        if (query.PageIndex < 0)
        {
            throw new ArgumentException("PageIndex must be zero or greater.", nameof(query.PageIndex));
        }

        if (query.PageSize is < 1 or > 100)
        {
            throw new ArgumentException("PageSize must be between 1 and 100.", nameof(query.PageSize));
        }
    }

    private static void ValidateRequest(UpsertCatalogItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Catalog item name is required.", nameof(request.Name));
        }

        if (request.Price < 0)
        {
            throw new ArgumentException("Catalog item price must be zero or greater.", nameof(request.Price));
        }

        if (request.AvailableStock < 0)
        {
            throw new ArgumentException("AvailableStock must be zero or greater.", nameof(request.AvailableStock));
        }
    }
}
