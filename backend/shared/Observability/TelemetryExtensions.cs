using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Observability;

public static class TelemetryExtensions
{
    /// <summary>
    /// Registers OpenTelemetry tracing + metrics. When Telemetry:OtlpEndpoint is set,
    /// traces are exported via OTLP (Jaeger / Tempo / OTel Collector). Prometheus
    /// metrics are always exposed (scrape at /metrics via MapPrometheusScrapingEndpoint).
    /// </summary>
    public static IServiceCollection AddPlatformTelemetry(
        this IServiceCollection services, string serviceName, IConfiguration? configuration = null)
    {
        var otlpEndpoint = configuration?.GetSection("Telemetry")["OtlpEndpoint"];

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
                }
                else
                {
                    tracing.AddConsoleExporter();
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddPrometheusExporter();
            });

        return services;
    }
}
