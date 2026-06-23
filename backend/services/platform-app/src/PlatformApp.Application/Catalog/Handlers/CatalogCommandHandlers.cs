using MediatR;
using PlatformApp.Application.Catalog.Commands;
using PlatformApp.Domain.Catalog;

namespace PlatformApp.Application.Catalog.Handlers;

public sealed class CreateCatalogItemHandler : IRequestHandler<CreateCatalogItemCommand, CatalogItem>
{
    private readonly CatalogService _service;

    public CreateCatalogItemHandler(CatalogService service) => _service = service;

    public Task<CatalogItem> Handle(CreateCatalogItemCommand request, CancellationToken cancellationToken)
    {
        var upsert = new UpsertCatalogItemRequest(
            request.Name,
            request.Description,
            request.Price,
            request.CatalogBrandId,
            request.CatalogTypeId,
            request.PictureUri,
            request.AvailableStock);

        return _service.CreateItemAsync(upsert, cancellationToken);
    }
}

public sealed class UpdateCatalogItemHandler : IRequestHandler<UpdateCatalogItemCommand, CatalogItem?>
{
    private readonly CatalogService _service;

    public UpdateCatalogItemHandler(CatalogService service) => _service = service;

    public Task<CatalogItem?> Handle(UpdateCatalogItemCommand request, CancellationToken cancellationToken)
    {
        var upsert = new UpsertCatalogItemRequest(
            request.Name,
            request.Description,
            request.Price,
            request.CatalogBrandId,
            request.CatalogTypeId,
            request.PictureUri,
            request.AvailableStock);

        return _service.UpdateItemAsync(request.ItemId, upsert, cancellationToken);
    }
}

public sealed class DeleteCatalogItemHandler : IRequestHandler<DeleteCatalogItemCommand, bool>
{
    private readonly CatalogService _service;

    public DeleteCatalogItemHandler(CatalogService service) => _service = service;

    public Task<bool> Handle(DeleteCatalogItemCommand request, CancellationToken cancellationToken)
        => _service.DeleteItemAsync(request.ItemId, cancellationToken);
}
