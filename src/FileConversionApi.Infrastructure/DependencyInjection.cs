// <copyright file="DependencyInjection.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Infrastructure.Converters;
using FileConversionApi.Infrastructure.HealthChecks;
using FileConversionApi.Infrastructure.Metrics;
using FileConversionApi.Infrastructure.Options;
using FileConversionApi.Infrastructure.Persistence;
using FileConversionApi.Infrastructure.Persistence.Repositories;
using FileConversionApi.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileConversionApi.Infrastructure;

/// <summary>
/// Dependency injection extensions for the Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure layer services to the container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        // Options
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<PuppeteerSettings>(configuration.GetSection(PuppeteerSettings.SectionName));
        services.Configure<WebhookSettings>(configuration.GetSection(WebhookSettings.SectionName));
        services.Configure<JobCleanupSettings>(configuration.GetSection(JobCleanupSettings.SectionName));
        services.Configure<HealthCheckSettings>(configuration.GetSection(HealthCheckSettings.SectionName));
        services.Configure<MetricsSettings>(configuration.GetSection(MetricsSettings.SectionName));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IConversionJobRepository, ConversionJobRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddHttpClient<IWebhookService, WebhookService>();
        services.AddHostedService<JobCleanupService>();
        services.AddSingleton<IMetricsService, PrometheusMetricsService>();

        // Converters
        services.AddSingleton<HtmlToPdfConverter>();
        services.AddSingleton<IFileConverter>(sp => sp.GetRequiredService<HtmlToPdfConverter>());
        services.AddSingleton<IFileConverter, MarkdownToPdfConverter>();
        services.AddSingleton<IFileConverter, MarkdownToHtmlConverter>();

        // Image converters
        services.AddSingleton<IFileConverter, PngToJpegConverter>();
        services.AddSingleton<IFileConverter, JpegToPngConverter>();
        services.AddSingleton<IFileConverter, PngToWebpConverter>();
        services.AddSingleton<IFileConverter, JpegToWebpConverter>();
        services.AddSingleton<IFileConverter, WebpToPngConverter>();
        services.AddSingleton<IFileConverter, WebpToJpegConverter>();

        services.AddSingleton<IConverterFactory, ConverterFactory>();

        // Health Checks
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"])
            .AddCheck<ChromiumHealthCheck>("chromium", tags: ["ready"])
            .AddCheck<DiskSpaceHealthCheck>("diskSpace", tags: ["ready"]);

        return services;
    }
}
