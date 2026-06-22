using MediatR;
using PlatformApp.Application.Orders.Queries;

namespace PlatformApp.Application.Orders.Handlers;

public sealed class GetMyOrdersHandler : IRequestHandler<GetMyOrdersQuery, IReadOnlyList<OrderSummaryResponse>>
{
    private readonly OrderService _service;
    public GetMyOrdersHandler(OrderService service) => _service = service;

    public Task<IReadOnlyList<OrderSummaryResponse>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
        => _service.GetMyOrdersAsync(request.UserId, cancellationToken);
}

public sealed class GetOrderDetailHandler : IRequestHandler<GetOrderDetailQuery, OrderDetailResponse?>
{
    private readonly OrderService _service;
    public GetOrderDetailHandler(OrderService service) => _service = service;

    public Task<OrderDetailResponse?> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
        => _service.GetOrderDetailAsync(request.UserId, request.OrderId, cancellationToken);
}
