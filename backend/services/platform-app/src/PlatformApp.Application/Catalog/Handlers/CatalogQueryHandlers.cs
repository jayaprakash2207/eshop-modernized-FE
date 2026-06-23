using MediatR;
using PlatformApp.Application.Catalog.Queries;

namespace PlatformApp.Application.Catalog.Handlers;

public sealed class GetCatalogItemsHandler : IRequestHandler<GetCatalogItemsQuery, CatalogItemsResponse>
{
    private readonly CatalogService _service;

    public GetCatalogItemsHandler(CatalogService service) => _service = service;

    public Task<CatalogItemsResponse> Handle(GetCatalogItemsQuery request, CancellationToken cancellationToken)
    {
        var query = new CatalogItemsQuery(request.PageIndex, request.PageSize, request.BrandId, request.TypeId);
        return _service.GetItemsAsync(query, cancellationToken);
    }
}

public sealed class GetCatalogItemByIdHandler : IRequestHandler<GetCatalogItemByIdQuery, CatalogItemDto?>
{
    private readonly CatalogService _service;

    public GetCatalogItemByIdHandler(CatalogService service) => _service = service;

    public Task<CatalogItemDto?> Handle(GetCatalogItemByIdQuery request, CancellationToken cancellationToken)
        => _service.GetItemAsync(request.ItemId, cancellationToken);
}

public sealed class GetCatalogBrandsHandler : IRequestHandler<GetCatalogBrandsQuery, IReadOnlyCollection<CatalogBrandDto>>
{
    private readonly CatalogService _service;

    public GetCatalogBrandsHandler(CatalogService service) => _service = service;

    public Task<IReadOnlyCollection<CatalogBrandDto>> Handle(GetCatalogBrandsQuery request, CancellationToken cancellationToken)
        => _service.GetBrandsAsync(cancellationToken);
}

public sealed class GetCatalogTypesHandler : IRequestHandler<GetCatalogTypesQuery, IReadOnlyCollection<CatalogTypeDto>>
{
    private readonly CatalogService _service;

    public GetCatalogTypesHandler(CatalogService service) => _service = service;

    public Task<IReadOnlyCollection<CatalogTypeDto>> Handle(GetCatalogTypesQuery request, CancellationToken cancellationToken)
        => _service.GetTypesAsync(cancellationToken);
}
