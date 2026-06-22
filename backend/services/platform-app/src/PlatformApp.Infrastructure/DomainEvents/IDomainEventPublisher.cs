namespace PlatformApp.Infrastructure.DomainEvents;

public interface IDomainEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default);
}
