using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PlatformApp.Infrastructure.Messaging;

/// <summary>
/// Registers MassTransit. When a RabbitMQ host is configured (Messaging:RabbitMqHost),
/// the bus uses the RabbitMQ transport for distributed saga choreography (KG: "RabbitMQ
/// (MassTransit) for saga choreography"). Otherwise it falls back to the in-memory
/// transport so local/dev and tests run without a broker.
/// </summary>
public static class MessagingRegistration
{
    public static IServiceCollection AddPlatformMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitHost = configuration.GetSection("Messaging")["RabbitMqHost"];

        services.AddMassTransit(bus =>
        {
            bus.SetKebabCaseEndpointNameFormatter();
            bus.AddConsumer<OrderPlacedConsumer>();

            if (!string.IsNullOrWhiteSpace(rabbitHost))
            {
                bus.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitHost, "/", h =>
                    {
                        h.Username(configuration.GetSection("Messaging")["RabbitMqUser"] ?? "guest");
                        h.Password(configuration.GetSection("Messaging")["RabbitMqPassword"] ?? "guest");
                    });
                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                bus.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            }
        });

        return services;
    }
}
