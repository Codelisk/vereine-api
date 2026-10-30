using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Vereine.ServiceDefaults;

/// <summary>
/// Grundausstattung jedes Vereine-Dienstes: OpenTelemetry (Logs, Metriken, Traces), Health Checks,
/// Service Discovery und resiliente HttpClients. Jeder Host registriert sie als Erstes.
/// </summary>
public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        // ASP.NET Core 11 liefert Server-Spans und HTTP-Metriken selbst nach OTel-Semantik.
        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics => metrics
                .AddMeter("Microsoft.AspNetCore.*")
                .AddMeter("Microsoft.Orleans")
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation())
            .WithTracing(tracing => tracing
                .AddSource(builder.Environment.ApplicationName)
                .AddSource("Microsoft.AspNetCore")
                .AddSource("Microsoft.Orleans.Runtime")
                .AddSource("Microsoft.Orleans.Application")
                .AddHttpClientInstrumentation());

        // Export nur, wenn ein Ziel gesetzt ist (Aspire setzt OTEL_EXPORTER_OTLP_ENDPOINT).
        if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: [HealthCheckTags.Live]);

        return builder;
    }

    /// <summary>
    /// <c>/alive</c> prüft nur den Prozess, <c>/health</c> zusätzlich alles mit Tag <see cref="HealthCheckTags.Ready"/>.
    /// Beide liefern nur den Status, keine Details.
    /// </summary>
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(HealthCheckTags.Live),
        }).ExcludeFromDescription();

        app.MapHealthChecks(HealthEndpointPath).ExcludeFromDescription();

        return app;
    }
}

/// <summary>Stabile Health-Tags.</summary>
public static class HealthCheckTags
{
    public const string Live = "live";
    public const string Ready = "ready";
}
