using MediatR;

namespace PlatformApp.Application.Basket.Queries;

public record GetBasketQuery(Guid BuyerId) : IRequest<BasketDto>;
