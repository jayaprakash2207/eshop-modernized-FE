using MediatR;
using PlatformApp.Application.Basket.Commands;
using PlatformApp.Application.Basket.Queries;

namespace PlatformApp.Application.Basket.Handlers;

public sealed class GetBasketHandler : IRequestHandler<GetBasketQuery, BasketResponse>
{
    private readonly IBasketRepository _repository;
    public GetBasketHandler(IBasketRepository repository) => _repository = repository;

    public Task<BasketResponse> Handle(GetBasketQuery request, CancellationToken cancellationToken)
        => _repository.GetAsync(request.UserId, cancellationToken);
}

public sealed class AddBasketItemHandler : IRequestHandler<AddBasketItemCommand, BasketResponse>
{
    private readonly BasketService _service;
    public AddBasketItemHandler(BasketService service) => _service = service;

    public Task<BasketResponse> Handle(AddBasketItemCommand request, CancellationToken cancellationToken)
        => _service.AddItemAsync(request.UserId, new AddBasketItemRequest(request.CatalogItemId, request.Quantity), cancellationToken);
}

public sealed class UpdateBasketItemHandler : IRequestHandler<UpdateBasketItemCommand, BasketResponse>
{
    private readonly BasketService _service;
    public UpdateBasketItemHandler(BasketService service) => _service = service;

    public Task<BasketResponse> Handle(UpdateBasketItemCommand request, CancellationToken cancellationToken)
        => _service.UpdateItemAsync(request.UserId, new UpdateBasketItemRequest(request.CatalogItemId, request.Quantity), cancellationToken);
}

public sealed class RemoveBasketItemHandler : IRequestHandler<RemoveBasketItemCommand, BasketResponse>
{
    private readonly BasketService _service;
    public RemoveBasketItemHandler(BasketService service) => _service = service;

    public Task<BasketResponse> Handle(RemoveBasketItemCommand request, CancellationToken cancellationToken)
        => _service.RemoveItemAsync(request.UserId, request.CatalogItemId, cancellationToken);
}

public sealed class CheckoutBasketHandler : IRequestHandler<CheckoutBasketCommand, CheckoutResult>
{
    private readonly BasketService _service;
    public CheckoutBasketHandler(BasketService service) => _service = service;

    public Task<CheckoutResult> Handle(CheckoutBasketCommand request, CancellationToken cancellationToken)
    {
        var checkoutRequest = new CheckoutRequest(
            request.ShipToAddress,
            request.City,
            request.State,
            request.ZipCode,
            request.Country);
        return _service.CheckoutAsync(request.UserId, request.Username, checkoutRequest, cancellationToken);
    }
}
