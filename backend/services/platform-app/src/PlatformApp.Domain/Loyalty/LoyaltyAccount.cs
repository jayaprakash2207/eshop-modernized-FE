using PlatformApp.Domain.Common;

namespace PlatformApp.Domain.Loyalty;

public sealed class LoyaltyAccount : Entity
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public long PointsBalance { get; private set; }
    public Guid? TierId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public LoyaltyAccount(Guid id, Guid userId)
    {
        Id = id;
        UserId = userId;
        PointsBalance = 0;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void CreditPoints(long points)
    {
        PointsBalance += points;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void DebitPoints(long points)
    {
        if (points > PointsBalance) throw new InvalidOperationException("Insufficient loyalty points.");
        PointsBalance -= points;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SetTier(Guid? tierId)
    {
        TierId = tierId;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
