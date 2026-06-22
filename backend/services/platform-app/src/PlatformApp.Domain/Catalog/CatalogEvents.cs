using PlatformApp.Domain.Common;

namespace PlatformApp.Domain.Catalog;

public sealed record CatalogItemCreatedDomainEvent(Guid AggregateId, string Name, DateTimeOffset OccurredAtUtc)
    : DomainEvent(AggregateId, OccurredAtUtc);

public sealed record CatalogItemUpdatedDomainEvent(Guid AggregateId, string Name, DateTimeOffset OccurredAtUtc)
    : DomainEvent(AggregateId, OccurredAtUtc);
