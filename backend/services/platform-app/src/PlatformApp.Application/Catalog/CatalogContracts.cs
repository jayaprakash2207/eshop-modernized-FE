namespace PlatformApp.Application.Catalog;

public sealed record CatalogBrandDto(Guid Id, string Name);

public sealed record CatalogTypeDto(Guid Id, string Name);

public sealed record CatalogItemDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    Guid CatalogBrandId,
    string CatalogBrand,
    Guid CatalogTypeId,
    string CatalogType,
    string PictureUri,
    int AvailableStock);

public sealed record CatalogItemsResponse(
    int PageIndex,
    int PageSize,
    int TotalCount,
    IReadOnlyCollection<CatalogItemDto> Items);

public sealed record CatalogItemsQuery(
    int PageIndex = 0,
    int PageSize = 10,
    Guid? CatalogBrandId = null,
    Guid? CatalogTypeId = null);

public sealed record UpsertCatalogItemRequest(
    string Name,
    string Description,
    decimal Price,
    Guid CatalogBrandId,
    Guid CatalogTypeId,
    string PictureUri,
    int AvailableStock);

public sealed record UpdateCatalogItemRequest(
    Guid CatalogItemId,
    string Name,
    string Description,
    decimal Price,
    Guid CatalogBrandId,
    Guid CatalogTypeId,
    string PictureUri,
    int AvailableStock);
