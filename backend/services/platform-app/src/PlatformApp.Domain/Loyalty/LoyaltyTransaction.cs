using PlatformApp.Domain.Common;

namespace PlatformApp.Domain.Loyalty;

public sealed class LoyaltyTransaction : Entity
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public string Type { get; private set; } // EARN, REDEEM, EXPIRE
    public long Points { get; private set; }
    public Guid? OrderId { get; private set; }
    public string? SourceEventId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? ExpiresAtUtc { get; private set; }

    public LoyaltyTransaction(Guid id, Guid accountId, string type, long points, Guid? orderId = null, string? sourceEventId = null, DateTimeOffset? expiresAt = null)
    {
        Id = id;
        AccountId = accountId;
        Type = type;
        Points = points;
        OrderId = orderId;
        SourceEventId = sourceEventId;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        ExpiresAtUtc = expiresAt;
    }
}
