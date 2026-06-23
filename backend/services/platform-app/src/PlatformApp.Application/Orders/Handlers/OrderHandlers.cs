using MediatR;
using PlatformApp.Application.Orders.Queries;

namespace PlatformApp.Application.Orders.Handlers;

public sealed class GetMyOrdersHandler : IRequestHandler<GetMyOrdersQuery, IReadOnlyCollection<OrderSummaryDto>>
{
    private readonly OrderService _service;
    public GetMyOrdersHandler(OrderService service) => _service = service;

    public Task<IReadOnlyCollection<OrderSummaryDto>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
        => _service.GetMyOrdersAsync(request.BuyerId, cancellationToken);
}

public sealed class GetOrderDetailHandler : IRequestHandler<GetOrderDetailQuery, OrderDetailDto?>
{
    private readonly OrderService _service;
    public GetOrderDetailHandler(OrderService service) => _service = service;

    public Task<OrderDetailDto?> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
        => _service.GetOrderDetailAsync(request.BuyerId, request.OrderId, cancellationToken);
}
