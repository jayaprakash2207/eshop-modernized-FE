using MediatR;
using PlatformApp.Application.Catalog;

namespace PlatformApp.Application.Catalog.Queries;

public record GetCatalogItemsQuery(int PageIndex, int PageSize, Guid? BrandId, Guid? TypeId)
    : IRequest<CatalogItemsResponse>;

public record GetCatalogItemByIdQuery(Guid ItemId) : IRequest<CatalogItemResponse?>;

public record GetCatalogBrandsQuery() : IRequest<IReadOnlyList<CatalogBrandResponse>>;

public record GetCatalogTypesQuery() : IRequest<IReadOnlyList<CatalogTypeResponse>>;
