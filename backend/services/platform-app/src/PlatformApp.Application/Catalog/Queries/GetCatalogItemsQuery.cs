using MediatR;

namespace PlatformApp.Application.Catalog.Queries;

public record GetCatalogItemsQuery(int PageIndex, int PageSize, Guid? BrandId, Guid? TypeId)
    : IRequest<CatalogItemsResponse>;

public record GetCatalogItemByIdQuery(Guid ItemId) : IRequest<CatalogItemDto?>;

public record GetCatalogBrandsQuery() : IRequest<IReadOnlyCollection<CatalogBrandDto>>;

public record GetCatalogTypesQuery() : IRequest<IReadOnlyCollection<CatalogTypeDto>>;
