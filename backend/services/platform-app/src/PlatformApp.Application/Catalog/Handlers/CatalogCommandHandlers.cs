using MediatR;
using PlatformApp.Application.Catalog.Commands;
using PlatformApp.Domain.Catalog;

namespace PlatformApp.Application.Catalog.Handlers;

public sealed class CreateCatalogItemHandler : IRequestHandler<CreateCatalogItemCommand, CatalogItemResponse>
{
    private readonly ICatalogRepository _repository;

    public CreateCatalogItemHandler(ICatalogRepository repository) => _repository = repository;

    public async Task<CatalogItemResponse> Handle(CreateCatalogItemCommand request, CancellationToken cancellationToken)
    {
        var item = new CatalogItem(
            Guid.NewGuid(),
            request.Name,
            request.Description,
            request.Price,
            request.CatalogBrandId,
            request.CatalogTypeId,
            request.PictureUri,
            request.AvailableStock);

        return await _repository.CreateItemAsync(item, cancellationToken);
    }
}

public sealed class UpdateCatalogItemHandler : IRequestHandler<UpdateCatalogItemCommand, CatalogItemResponse?>
{
    private readonly ICatalogRepository _repository;

    public UpdateCatalogItemHandler(ICatalogRepository repository) => _repository = repository;

    public async Task<CatalogItemResponse?> Handle(UpdateCatalogItemCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetItemByIdAsync(request.ItemId, cancellationToken);
        if (existing is null) return null;

        var upsert = new UpsertCatalogItemRequest(
            request.Name,
            request.Description,
            request.Price,
            request.CatalogBrandId,
            request.CatalogTypeId,
            request.PictureUri,
            request.AvailableStock);

        return await _repository.UpdateItemAsync(request.ItemId, upsert, cancellationToken);
    }
}

public sealed class DeleteCatalogItemHandler : IRequestHandler<DeleteCatalogItemCommand, bool>
{
    private readonly ICatalogRepository _repository;

    public DeleteCatalogItemHandler(ICatalogRepository repository) => _repository = repository;

    public Task<bool> Handle(DeleteCatalogItemCommand request, CancellationToken cancellationToken)
        => _repository.DeleteItemAsync(request.ItemId, cancellationToken);
}
