using MediatR;

namespace PlatformApp.Application.Orders.Queries;

public record GetMyOrdersQuery(Guid BuyerId) : IRequest<IReadOnlyCollection<OrderSummaryDto>>;

public record GetOrderDetailQuery(Guid BuyerId, Guid OrderId) : IRequest<OrderDetailDto?>;
