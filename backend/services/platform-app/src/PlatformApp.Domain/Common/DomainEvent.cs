namespace PlatformApp.Domain.Common;

public abstract record DomainEvent(Guid AggregateId, DateTimeOffset OccurredAtUtc);
