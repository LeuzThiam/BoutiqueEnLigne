using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BoutiqueEnLigne.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddServiceObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live", "ready"]);

        var endpoint = configuration["OpenTelemetry:OtlpEndpoint"];
        var telemetry = services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: serviceName, serviceVersion: "1.0.0")
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production"
                }));

        telemetry.WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation(options =>
                options.Filter = context => !context.Request.Path.StartsWithSegments("/health"));
            tracing.AddHttpClientInstrumentation();
            tracing.AddSource("BoutiqueEnLigne.*");
            if (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
            {
                tracing.AddOtlpExporter(options => options.Endpoint = uri);
            }
        });

        telemetry.WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation();
            metrics.AddHttpClientInstrumentation();
            metrics.AddRuntimeInstrumentation();
            if (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
            {
                metrics.AddOtlpExporter(options => options.Endpoint = uri);
            }
        });

        return services;
    }

    public static WebApplication UseServiceObservability(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("live")
        });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready")
        });
        return app;
    }
}
