namespace PlatformApp.Application.Loyalty;

public sealed record PointsEarnedEvent(Guid AccountId, long Points, Guid? OrderId, string? SourceEventId);
public sealed record PointsRedeemedEvent(Guid AccountId, long Points, Guid? OrderId, string? SourceEventId);
