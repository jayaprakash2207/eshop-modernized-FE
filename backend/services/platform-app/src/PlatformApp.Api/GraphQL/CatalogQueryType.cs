using MediatR;
using PlatformApp.Application.Catalog;
using PlatformApp.Application.Catalog.Queries;

namespace PlatformApp.Api.GraphQL;

/// <summary>
/// GraphQL query root. Resolvers dispatch through the same MediatR CQRS handlers
/// used by the REST endpoints, so REST and GraphQL share one read model.
/// KG target stack: "GraphQL (HotChocolate)".
/// </summary>
public sealed class Query
{
    public async Task<CatalogItemsResponse> GetCatalogItems(
        [Service] ISender sender,
        int pageIndex = 0,
        int pageSize = 10,
        Guid? brandId = null,
        Guid? typeId = null)
        => await sender.Send(new GetCatalogItemsQuery(pageIndex, pageSize, brandId, typeId));

    public async Task<CatalogItemDto?> GetCatalogItemById([Service] ISender sender, Guid id)
        => await sender.Send(new GetCatalogItemByIdQuery(id));

    public async Task<IReadOnlyCollection<CatalogBrandDto>> GetCatalogBrands([Service] ISender sender)
        => await sender.Send(new GetCatalogBrandsQuery());

    public async Task<IReadOnlyCollection<CatalogTypeDto>> GetCatalogTypes([Service] ISender sender)
        => await sender.Send(new GetCatalogTypesQuery());
}
