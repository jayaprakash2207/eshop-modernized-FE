using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlatformApp.Application.Catalog;
using PlatformApp.Infrastructure.Caching;
using PlatformApp.Infrastructure.Messaging;
using PlatformApp.Application.Basket;
using PlatformApp.Application.Identity;
using PlatformApp.Application.Orders;
using PlatformApp.Application.Payments;
using PlatformApp.Infrastructure.Basket;
using PlatformApp.Infrastructure.Catalog;
using PlatformApp.Infrastructure.Identity;
using PlatformApp.Infrastructure.Orders;
using PlatformApp.Infrastructure.Payments;
using PlatformApp.Infrastructure.Persistence;
using PlatformApp.Infrastructure.State;
using Microsoft.Extensions.Options;

namespace PlatformApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<PersistenceOptions>(configuration.GetSection(PersistenceOptions.SectionName));
        services.AddSingleton<PostgreSqlConnectionFactory>();
        services.AddSingleton(_ => AppStateSeeder.Seed());

        var persistenceOptions = configuration.GetSection(PersistenceOptions.SectionName).Get<PersistenceOptions>() ?? new PersistenceOptions();
        var usePostgres = string.Equals(persistenceOptions.Provider, "PostgreSql", StringComparison.OrdinalIgnoreCase);

        if (usePostgres)
        {
            services.AddSingleton<PostgreSqlCatalogRepository>();
            services.AddSingleton<ICatalogRepository>(sp => DecorateCatalog(sp, sp.GetRequiredService<PostgreSqlCatalogRepository>()));
            services.AddSingleton<IBasketRepository, PostgreSqlBasketRepository>();
            services.AddSingleton<IOrderRepository, PostgreSqlOrderRepository>();
            services.AddSingleton<IIdentityService, PostgreSqlIdentityAccountService>();
            services.AddSingleton<IIdentityAccountService, PostgreSqlIdentityAccountService>();
            services.AddSingleton<PlatformApp.Application.Loyalty.ILoyaltyRepository, PlatformApp.Infrastructure.Loyalty.PostgreSqlLoyaltyRepository>();
        }
        else
        {
            services.AddSingleton<InMemoryCatalogRepository>();
            services.AddSingleton<ICatalogRepository>(sp => DecorateCatalog(sp, sp.GetRequiredService<InMemoryCatalogRepository>()));
            services.AddSingleton<IBasketRepository, InMemoryBasketRepository>();
            services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
            services.AddSingleton<IIdentityService, JwtIdentityService>();
            services.AddSingleton<IIdentityAccountService, InMemoryIdentityAccountService>();
            services.AddSingleton<PlatformApp.Application.Loyalty.ILoyaltyRepository, PlatformApp.Infrastructure.Loyalty.InMemoryLoyaltyRepository>();
        }

        // ── Distributed cache (Redis in prod, in-memory fallback) ──────────────────
        var redisConnection = configuration.GetSection("Cache")["RedisConnection"];
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "platformapp:";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddSingleton<PlatformApp.Application.Abstractions.IDomainEventPublisher, PlatformApp.Infrastructure.DomainEvents.InMemoryDomainEventPublisher>();

        // Refresh-token rotation service (issue + rotate + revoke)
        services.AddSingleton<IRefreshTokenService, InMemoryRefreshTokenService>();

        // Message bus (RabbitMQ / in-memory) + integration-event publisher
        services.AddPlatformMessaging(configuration);
        services.AddSingleton<PlatformApp.Application.Abstractions.IIntegrationEventPublisher,
                              PlatformApp.Infrastructure.Messaging.MassTransitIntegrationEventPublisher>();

        services.AddSingleton<IPaymentService, InMemoryPaymentService>();
        return services;
    }

    // Wraps the concrete catalog repository in the Redis/in-memory caching decorator.
    private static ICatalogRepository DecorateCatalog(IServiceProvider sp, ICatalogRepository inner)
        => new CachedCatalogRepository(inner, sp.GetRequiredService<IDistributedCache>());
}
