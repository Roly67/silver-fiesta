// <copyright file="DependencyInjection.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Infrastructure.Converters;
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

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IConversionJobRepository, ConversionJobRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddHttpClient<IWebhookService, WebhookService>();

        // Converters
        services.AddSingleton<HtmlToPdfConverter>();
        services.AddSingleton<IFileConverter>(sp => sp.GetRequiredService<HtmlToPdfConverter>());
        services.AddSingleton<IFileConverter, MarkdownToPdfConverter>();
        services.AddSingleton<IConverterFactory, ConverterFactory>();

        return services;
    }
}
