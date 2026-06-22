using MediatR;
using PlatformApp.Application.Catalog.Queries;

namespace PlatformApp.Application.Catalog.Handlers;

public sealed class GetCatalogItemsHandler : IRequestHandler<GetCatalogItemsQuery, CatalogItemsResponse>
{
    private readonly ICatalogRepository _repository;

    public GetCatalogItemsHandler(ICatalogRepository repository) => _repository = repository;

    public async Task<CatalogItemsResponse> Handle(GetCatalogItemsQuery request, CancellationToken cancellationToken)
    {
        var query = new CatalogItemsQuery(request.PageIndex, request.PageSize, request.BrandId, request.TypeId);
        return await _repository.GetItemsPagedAsync(query, cancellationToken);
    }
}

public sealed class GetCatalogItemByIdHandler : IRequestHandler<GetCatalogItemByIdQuery, CatalogItemResponse?>
{
    private readonly ICatalogRepository _repository;

    public GetCatalogItemByIdHandler(ICatalogRepository repository) => _repository = repository;

    public Task<CatalogItemResponse?> Handle(GetCatalogItemByIdQuery request, CancellationToken cancellationToken)
        => _repository.GetItemByIdAsync(request.ItemId, cancellationToken);
}

public sealed class GetCatalogBrandsHandler : IRequestHandler<GetCatalogBrandsQuery, IReadOnlyList<CatalogBrandResponse>>
{
    private readonly ICatalogRepository _repository;

    public GetCatalogBrandsHandler(ICatalogRepository repository) => _repository = repository;

    public Task<IReadOnlyList<CatalogBrandResponse>> Handle(GetCatalogBrandsQuery request, CancellationToken cancellationToken)
        => _repository.GetBrandsAsync(cancellationToken);
}

public sealed class GetCatalogTypesHandler : IRequestHandler<GetCatalogTypesQuery, IReadOnlyList<CatalogTypeResponse>>
{
    private readonly ICatalogRepository _repository;

    public GetCatalogTypesHandler(ICatalogRepository repository) => _repository = repository;

    public Task<IReadOnlyList<CatalogTypeResponse>> Handle(GetCatalogTypesQuery request, CancellationToken cancellationToken)
        => _repository.GetTypesAsync(cancellationToken);
}
