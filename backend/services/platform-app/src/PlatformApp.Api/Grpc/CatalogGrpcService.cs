using Grpc.Core;
using MediatR;
using PlatformApp.Application.Catalog.Queries;

namespace PlatformApp.Api.Grpc;

/// <summary>
/// gRPC service implementation backed by the same MediatR CQRS handlers as REST/GraphQL.
/// The CatalogGrpcBase type is generated from Protos/catalog.proto at build time.
/// </summary>
public sealed class CatalogGrpcService : CatalogGrpc.CatalogGrpcBase
{
    private readonly ISender _sender;

    public CatalogGrpcService(ISender sender) => _sender = sender;

    public override async Task<CatalogItemMessage> GetItemById(GetItemByIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ItemId, out var id))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "item_id must be a GUID"));
        }

        var item = await _sender.Send(new GetCatalogItemByIdQuery(id), context.CancellationToken);
        if (item is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Catalog item not found"));
        }

        return new CatalogItemMessage
        {
            Id             = item.Id.ToString(),
            Name           = item.Name,
            Description     = item.Description,
            Price          = (double)item.Price,
            Brand          = item.CatalogBrand,
            Type           = item.CatalogType,
            AvailableStock = item.AvailableStock
        };
    }

    public override async Task<BrandListMessage> GetBrands(GetBrandsRequest request, ServerCallContext context)
    {
        var brands = await _sender.Send(new GetCatalogBrandsQuery(), context.CancellationToken);
        var message = new BrandListMessage();
        message.Brands.AddRange(brands.Select(b => new BrandMessage { Id = b.Id.ToString(), Name = b.Name }));
        return message;
    }
}
