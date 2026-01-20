using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace Observability;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddObservabilityExt(this IServiceCollection services, IConfiguration configuration)

    {
        services.Configure<OpenTelemetryConstants>(configuration.GetSection("OpenTelemetry"));
        var openTelemetryConstants = configuration.GetSection("OpenTelemetry").Get<OpenTelemetryConstants>()!;

        ActivitySourceProvider.Source =
            new ActivitySource(openTelemetryConstants.ActivitySourceName);

        services.AddOpenTelemetry().WithTracing(options =>
        {
            options.AddSource(openTelemetryConstants.ActivitySourceName)
                .ConfigureResource(resource =>
                {
                    resource.AddService(openTelemetryConstants.ServiceName,
                        serviceVersion: openTelemetryConstants.ServiceVersion);
                });
            options.AddAspNetCoreInstrumentation(aspnetcoreOptions =>
            {
                aspnetcoreOptions.Filter = context =>
                {
                    if (!string.IsNullOrEmpty(context.Request.Path.Value))
                        return context.Request.Path.Value.Contains("api", StringComparison.InvariantCulture);

                    return false;
                };
            });

            options.AddEntityFrameworkCoreInstrumentation();

            options.AddHttpClientInstrumentation();
            options.AddConsoleExporter();
            options.AddOtlpExporter();
        }).WithMetrics(metricsBuilder =>
        {
            metricsBuilder.AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation();
            metricsBuilder.ConfigureResource(configure =>
            {
                configure.AddService(openTelemetryConstants.ServiceName,
                    serviceVersion: openTelemetryConstants.ServiceVersion);
            });

            metricsBuilder.AddConsoleExporter();
            metricsBuilder.AddOtlpExporter();
        }).WithLogging(loggingBuilder =>
        {
            loggingBuilder.ConfigureResource(configure =>
            {
                configure.AddService(openTelemetryConstants.ServiceName,
                    serviceVersion: openTelemetryConstants.ServiceVersion);
            });
            loggingBuilder.AddConsoleExporter();
            loggingBuilder.AddOtlpExporter();
        });


        return services;
    }
}