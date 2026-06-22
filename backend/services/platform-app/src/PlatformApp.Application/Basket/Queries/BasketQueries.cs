using MediatR;

namespace PlatformApp.Application.Basket.Queries;

public record GetBasketQuery(Guid UserId) : IRequest<BasketResponse>;
