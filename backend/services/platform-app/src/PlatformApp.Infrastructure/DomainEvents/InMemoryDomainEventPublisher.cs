using Microsoft.Extensions.Logging;
using PlatformApp.Application.Abstractions;

namespace PlatformApp.Infrastructure.DomainEvents;

public sealed class InMemoryDomainEventPublisher : IDomainEventPublisher
{
    private readonly ILogger<InMemoryDomainEventPublisher> _logger;

    public InMemoryDomainEventPublisher(ILogger<InMemoryDomainEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Domain event published: {Event}", @event?.ToString());
        return Task.CompletedTask;
    }
}
