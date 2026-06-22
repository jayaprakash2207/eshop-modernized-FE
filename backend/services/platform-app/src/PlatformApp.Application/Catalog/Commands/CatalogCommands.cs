using MediatR;
using PlatformApp.Application.Catalog;

namespace PlatformApp.Application.Catalog.Commands;

public record CreateCatalogItemCommand(
    string Name,
    string Description,
    decimal Price,
    Guid CatalogBrandId,
    Guid CatalogTypeId,
    string PictureUri,
    int AvailableStock) : IRequest<CatalogItemResponse>;

public record UpdateCatalogItemCommand(
    Guid ItemId,
    string Name,
    string Description,
    decimal Price,
    Guid CatalogBrandId,
    Guid CatalogTypeId,
    string PictureUri,
    int AvailableStock) : IRequest<CatalogItemResponse?>;

public record DeleteCatalogItemCommand(Guid ItemId) : IRequest<bool>;
