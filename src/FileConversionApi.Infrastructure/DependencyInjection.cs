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
        services.Configure<InputValidationSettings>(configuration.GetSection(InputValidationSettings.SectionName));
        services.Configure<AdminSeedSettings>(configuration.GetSection(AdminSeedSettings.SectionName));
        services.Configure<UsageQuotaSettings>(configuration.GetSection(UsageQuotaSettings.SectionName));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IConversionJobRepository, ConversionJobRepository>();
        services.AddScoped<IConversionTemplateRepository, ConversionTemplateRepository>();
        services.AddScoped<IUsageQuotaRepository, UsageQuotaRepository>();
        services.AddScoped<IUserRateLimitSettingsRepository, UserRateLimitSettingsRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddHttpClient<IWebhookService, WebhookService>();
        services.AddHostedService<JobCleanupService>();
        services.AddHostedService<AdminSeederService>();
        services.AddSingleton<IMetricsService, PrometheusMetricsService>();
        services.AddSingleton<IPdfWatermarkService, PdfWatermarkService>();
        services.AddSingleton<IPdfEncryptionService, PdfEncryptionService>();
        services.AddSingleton<IPdfManipulationService, PdfManipulationService>();
        services.AddSingleton<IInputValidationService, InputValidationService>();
        services.AddScoped<IUsageQuotaService, UsageQuotaService>();
        services.AddScoped<IUserRateLimitService, UserRateLimitService>();

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
