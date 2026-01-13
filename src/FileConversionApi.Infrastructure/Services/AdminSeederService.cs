// <copyright file="AdminSeederService.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;
using FileConversionApi.Domain.Entities;
using FileConversionApi.Infrastructure.Options;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileConversionApi.Infrastructure.Services;

/// <summary>
/// Hosted service that seeds the default admin user on application startup.
/// </summary>
public class AdminSeederService : IHostedService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly AdminSeedSettings settings;
    private readonly ILogger<AdminSeederService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminSeederService"/> class.
    /// </summary>
    /// <param name="scopeFactory">The service scope factory.</param>
    /// <param name="settings">The admin seed settings.</param>
    /// <param name="logger">The logger.</param>
    public AdminSeederService(
        IServiceScopeFactory scopeFactory,
        IOptions<AdminSeedSettings> settings,
        ILogger<AdminSeederService> logger)
    {
        this.scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        this.settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!this.settings.Enabled)
        {
            this.logger.LogDebug("Admin seeding is disabled");
            return;
        }

        await this.SeedAdminUserAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// Seeds the default admin user if it doesn't already exist.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    internal async Task SeedAdminUserAsync(CancellationToken cancellationToken)
    {
        using var scope = this.scopeFactory.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            // Check if we should skip seeding when any admin already exists
            if (this.settings.SkipIfAdminExists)
            {
                var adminExists = await userRepository.AnyAdminExistsAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (adminExists)
                {
                    this.logger.LogDebug(
                        "Admin seeding skipped: an admin user already exists");
                    return;
                }
            }

            // Check if the email is already in use
            var emailExists = await userRepository.EmailExistsAsync(this.settings.Email, cancellationToken)
                .ConfigureAwait(false);

            if (emailExists)
            {
                this.logger.LogWarning(
                    "Admin seeding skipped: email {Email} is already in use",
                    this.settings.Email);
                return;
            }

            // Create the admin user
            var passwordHash = passwordHasher.Hash(this.settings.Password);
            var adminUser = User.Create(this.settings.Email, passwordHash);
            adminUser.GrantAdmin();

            await userRepository.AddAsync(adminUser, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            this.logger.LogInformation(
                "Admin user seeded successfully with email {Email}",
                this.settings.Email);
        }
        catch (Exception ex) when (ex is not (OutOfMemoryException or StackOverflowException))
        {
            this.logger.LogError(ex, "Failed to seed admin user");
        }
    }
}
