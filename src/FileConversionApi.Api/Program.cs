// <copyright file="Program.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

using FileConversionApi.Api.HealthChecks;
using FileConversionApi.Api.Middleware;
using FileConversionApi.Api.Services;
using FileConversionApi.Application;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Infrastructure;
using FileConversionApi.Infrastructure.Options;
using FileConversionApi.Infrastructure.Persistence;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Prometheus;

using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting FileConversionApi");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // Add services to the container.
    builder.Services.AddMemoryCache();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    // Configure Authentication
    var jwtSecret = builder.Configuration["JwtSettings:Secret"]
        ?? throw new InvalidOperationException("JWT Secret is not configured.");
    var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]
        ?? throw new InvalidOperationException("JWT Issuer is not configured.");
    var jwtAudience = builder.Configuration["JwtSettings:Audience"]
        ?? throw new InvalidOperationException("JWT Audience is not configured.");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero,
        };
    })
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", null);

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    });

    // Configure Rate Limiting
    builder.Services.Configure<RateLimitingSettings>(
        builder.Configuration.GetSection(RateLimitingSettings.SectionName));

    var rateLimitSettings = builder.Configuration
        .GetSection(RateLimitingSettings.SectionName)
        .Get<RateLimitingSettings>() ?? new RateLimitingSettings();

    if (rateLimitSettings.EnableRateLimiting)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Standard policy for general endpoints (per-user dynamic limits)
            options.AddPolicy("standard", context =>
                CreateUserAwarePartition(context, "standard", rateLimitSettings));

            // Conversion policy for resource-intensive endpoints (per-user dynamic limits)
            options.AddPolicy("conversion", context =>
                CreateUserAwarePartition(context, "conversion", rateLimitSettings));

            // Auth policy for authentication endpoints (uses IP-based partitioning)
            options.AddPolicy("auth", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitSettings.AuthPolicy.PermitLimit,
                        Window = TimeSpan.FromMinutes(rateLimitSettings.AuthPolicy.WindowMinutes),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                    }));

            // Custom response for rate limit exceeded
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/problem+json";

                var retryAfterSeconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                    ? (int)retryAfter.TotalSeconds
                    : 60;

                context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString();

                var problem = new
                {
                    type = "https://httpstatuses.com/429",
                    title = "Too Many Requests",
                    status = 429,
                    detail = "Rate limit exceeded. Please try again later.",
                    retryAfter = retryAfterSeconds,
                };

                await context.HttpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            };
        });
    }

    // Configure OpenTelemetry Tracing
    var otelSettings = builder.Configuration
        .GetSection(OpenTelemetrySettings.SectionName)
        .Get<OpenTelemetrySettings>() ?? new OpenTelemetrySettings();

    if (otelSettings.EnableTracing)
    {
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: otelSettings.ServiceName,
                    serviceVersion: otelSettings.ServiceVersion ?? typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0"))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.Filter = context =>
                        {
                            // Exclude health check and metrics endpoints from tracing
                            var path = context.Request.Path.Value ?? string.Empty;
                            return !path.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
                                && !path.StartsWith("/metrics", StringComparison.OrdinalIgnoreCase);
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddSource("FileConversionApi.Conversions");

                // Configure sampling
                if (otelSettings.SamplingRatio < 1.0)
                {
                    tracing.SetSampler(new TraceIdRatioBasedSampler(otelSettings.SamplingRatio));
                }

                // Configure exporters
                if (!string.IsNullOrEmpty(otelSettings.OtlpEndpoint))
                {
                    tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otelSettings.OtlpEndpoint);
                    });
                }

                if (otelSettings.ExportToConsole)
                {
                    tracing.AddConsoleExporter();
                }
            });
    }

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "File Conversion API",
            Version = "v1",
            Description = "API for converting files between different formats",
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
        });

        options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
        {
            Description = "API Key authentication using X-API-Key header.",
            Name = "X-API-Key",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
        });

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>(),
            [new OpenApiSecuritySchemeReference("ApiKey", document)] = new List<string>(),
        });

        var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "File Conversion API v1");
        });

        // Apply migrations in development
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    app.UseExceptionHandler();

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseRateLimiter();
    app.UseAuthorization();

    // Prometheus HTTP metrics middleware
    app.UseHttpMetrics();

    app.MapControllers();

    // Prometheus metrics endpoint
    app.MapMetrics();

    // Health check endpoints
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = HealthCheckResponseWriter.WriteAsync,
    });

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false, // Liveness: just check if app is running
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = HealthCheckResponseWriter.WriteAsync,
    });

    await app.RunAsync();

    // Helper method to get partition key for rate limiting
    static string GetPartitionKey(HttpContext context)
    {
        // Use user ID for authenticated requests, IP for anonymous
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userId ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
    }

    // Helper method to create user-aware rate limit partition with dynamic limits
    static RateLimitPartition<string> CreateUserAwarePartition(
        HttpContext context,
        string policyName,
        RateLimitingSettings settings)
    {
        var partitionKey = GetPartitionKey(context);
        var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = context.User?.IsInRole("Admin") ?? false;

        // If admin exemption is enabled and user is admin, use a very high limit
        if (settings.ExemptAdmins && isAdmin)
        {
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: $"admin:{partitionKey}",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = int.MaxValue,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0,
                });
        }

        // For authenticated users, try to get their effective limits
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
        {
            var rateLimitService = context.RequestServices.GetService<IUserRateLimitService>();
            if (rateLimitService != null)
            {
                // Get effective limits synchronously (the service uses cached values)
                var effectiveLimits = rateLimitService
                    .GetEffectiveLimitsAsync(new FileConversionApi.Domain.ValueObjects.UserId(userId), policyName, CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: partitionKey,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = effectiveLimits.PermitLimit,
                        Window = effectiveLimits.Window,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                    });
            }
        }

        // Fall back to default policy settings for anonymous users or if service unavailable
        var defaultPolicy = policyName.Equals("conversion", StringComparison.OrdinalIgnoreCase)
            ? settings.ConversionPolicy
            : settings.StandardPolicy;

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: partitionKey,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = defaultPolicy.PermitLimit,
                Window = TimeSpan.FromMinutes(defaultPolicy.WindowMinutes),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
            });
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
