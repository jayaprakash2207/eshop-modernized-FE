using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlatformApp.Application.Catalog;
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
            services.AddSingleton<ICatalogRepository, PostgreSqlCatalogRepository>();
            services.AddSingleton<IBasketRepository, PostgreSqlBasketRepository>();
            services.AddSingleton<IOrderRepository, PostgreSqlOrderRepository>();
            services.AddSingleton<IIdentityService, PostgreSqlIdentityAccountService>();
            services.AddSingleton<IIdentityAccountService, PostgreSqlIdentityAccountService>();
            services.AddSingleton<PlatformApp.Application.Loyalty.ILoyaltyRepository, PlatformApp.Infrastructure.Loyalty.PostgreSqlLoyaltyRepository>();
        }
        else
        {
            services.AddSingleton<ICatalogRepository, InMemoryCatalogRepository>();
            services.AddSingleton<IBasketRepository, InMemoryBasketRepository>();
            services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
            services.AddSingleton<IIdentityService, JwtIdentityService>();
            services.AddSingleton<IIdentityAccountService, InMemoryIdentityAccountService>();
            services.AddSingleton<PlatformApp.Application.Loyalty.ILoyaltyRepository, PlatformApp.Infrastructure.Loyalty.InMemoryLoyaltyRepository>();
        }

        services.AddSingleton<PlatformApp.Infrastructure.DomainEvents.IDomainEventPublisher, PlatformApp.Infrastructure.DomainEvents.InMemoryDomainEventPublisher>();

        services.AddSingleton<IPaymentService, InMemoryPaymentService>();
        return services;
    }
}
