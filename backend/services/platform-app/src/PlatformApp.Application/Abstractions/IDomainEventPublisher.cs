namespace PlatformApp.Application.Abstractions;

/// <summary>
/// Application-layer abstraction for publishing in-process domain events.
/// Implemented in Infrastructure. Lives here (not in Infrastructure) so the
/// Application layer never depends on Infrastructure — Clean Architecture
/// dependency rule: dependencies point inward only.
/// </summary>
public interface IDomainEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default);
}
