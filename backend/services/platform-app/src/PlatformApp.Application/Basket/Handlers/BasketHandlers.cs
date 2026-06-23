using MediatR;
using PlatformApp.Application.Basket.Commands;
using PlatformApp.Application.Basket.Queries;

namespace PlatformApp.Application.Basket.Handlers;

public sealed class GetBasketHandler : IRequestHandler<GetBasketQuery, BasketDto>
{
    private readonly BasketService _service;
    public GetBasketHandler(BasketService service) => _service = service;

    public Task<BasketDto> Handle(GetBasketQuery request, CancellationToken cancellationToken)
        => _service.GetAsync(request.BuyerId, cancellationToken);
}

public sealed class AddBasketItemHandler : IRequestHandler<AddBasketItemCommand, BasketDto>
{
    private readonly BasketService _service;
    public AddBasketItemHandler(BasketService service) => _service = service;

    public Task<BasketDto> Handle(AddBasketItemCommand request, CancellationToken cancellationToken)
        => _service.AddItemAsync(request.BuyerId, new AddBasketItemRequest(request.CatalogItemId, request.Quantity), cancellationToken);
}

public sealed class UpdateBasketItemHandler : IRequestHandler<UpdateBasketItemCommand, BasketDto>
{
    private readonly BasketService _service;
    public UpdateBasketItemHandler(BasketService service) => _service = service;

    public Task<BasketDto> Handle(UpdateBasketItemCommand request, CancellationToken cancellationToken)
        => _service.UpdateItemAsync(request.BuyerId, new UpdateBasketItemRequest(request.CatalogItemId, request.Quantity), cancellationToken);
}

public sealed class RemoveBasketItemHandler : IRequestHandler<RemoveBasketItemCommand, BasketDto>
{
    private readonly BasketService _service;
    public RemoveBasketItemHandler(BasketService service) => _service = service;

    public Task<BasketDto> Handle(RemoveBasketItemCommand request, CancellationToken cancellationToken)
        => _service.RemoveItemAsync(request.BuyerId, request.CatalogItemId, cancellationToken);
}

public sealed class CheckoutBasketHandler : IRequestHandler<CheckoutBasketCommand, CheckoutResult>
{
    private readonly BasketService _service;
    public CheckoutBasketHandler(BasketService service) => _service = service;

    public Task<CheckoutResult> Handle(CheckoutBasketCommand request, CancellationToken cancellationToken)
    {
        var checkoutRequest = new CheckoutRequest(
            request.Street,
            request.City,
            request.State,
            request.PostalCode,
            request.Country);
        return _service.CheckoutAsync(request.BuyerId, request.Username, checkoutRequest, cancellationToken);
    }
}
