using System.Threading;
using PlatformApp.Application.Loyalty;
using PlatformApp.Infrastructure.Loyalty;
using PlatformApp.Infrastructure.State;
using PlatformApp.Infrastructure.DomainEvents;
using Xunit;

namespace PlatformApp.Loyalty.Tests;

public class LoyaltyServiceTests
{
    [Fact]
    public async Task EarnAndRedeem_Works_WithInMemoryRepo()
    {
        var state = AppStateSeeder.Seed();
        var repo = new InMemoryLoyaltyRepository(state);
        var publisher = new InMemoryDomainEventPublisher(new Microsoft.Extensions.Logging.Abstractions.NullLogger<InMemoryDomainEventPublisher>());
        var service = new LoyaltyService(repo, publisher);

        var userId = Guid.NewGuid();
        var earnRequest = new EarnPointsRequest(Guid.NewGuid(), 100m, null);
        var points = await service.EarnPointsAsync(userId, earnRequest, CancellationToken.None);
        Assert.True(points > 0);

        var balance = await service.GetBalanceAsync(userId, CancellationToken.None);
        Assert.Equal(points, balance.PointsBalance);

        var redeemRequest = new RedeemPointsRequest(balance.PointsBalance > 0 ? balance.PointsBalance : 0, Math.Min(10, balance.PointsBalance));
        await service.RedeemPointsAsync(userId, redeemRequest, CancellationToken.None);

        var newBalance = await service.GetBalanceAsync(userId, CancellationToken.None);
        Assert.Equal(balance.PointsBalance - redeemRequest.Points, newBalance.PointsBalance);
    }
}
