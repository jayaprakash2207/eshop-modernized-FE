using MediatR;

namespace PlatformApp.Application.Orders.Queries;

public record GetMyOrdersQuery(Guid UserId) : IRequest<IReadOnlyList<OrderSummaryResponse>>;

public record GetOrderDetailQuery(Guid UserId, Guid OrderId) : IRequest<OrderDetailResponse?>;
