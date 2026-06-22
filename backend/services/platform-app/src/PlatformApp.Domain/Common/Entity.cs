namespace PlatformApp.Domain.Common;

public abstract class Entity
{
    private readonly List<DomainEvent> _domainEvents = [];

    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAtUtc { get; protected set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; protected set; } = DateTimeOffset.UtcNow;

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void RestoreIdentity(Guid id, DateTimeOffset createdAtUtc, DateTimeOffset updatedAtUtc)
    {
        Id = id;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }
}
