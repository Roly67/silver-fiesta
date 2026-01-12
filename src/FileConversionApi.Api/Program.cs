// <copyright file="Program.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

using FileConversionApi.Api.Middleware;
using FileConversionApi.Api.Services;
using FileConversionApi.Application;
using FileConversionApi.Application.Interfaces;
using FileConversionApi.Infrastructure;
using FileConversionApi.Infrastructure.Options;
using FileConversionApi.Infrastructure.Persistence;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

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

    builder.Services.AddAuthorization();

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

            // Standard policy for general endpoints
            options.AddPolicy("standard", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetPartitionKey(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitSettings.StandardPolicy.PermitLimit,
                        Window = TimeSpan.FromMinutes(rateLimitSettings.StandardPolicy.WindowMinutes),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                    }));

            // Conversion policy for resource-intensive endpoints
            options.AddPolicy("conversion", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetPartitionKey(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitSettings.ConversionPolicy.PermitLimit,
                        Window = TimeSpan.FromMinutes(rateLimitSettings.ConversionPolicy.WindowMinutes),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                    }));

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

    app.MapControllers();

    await app.RunAsync();

    // Helper method to get partition key for rate limiting
    static string GetPartitionKey(HttpContext context)
    {
        // Use user ID for authenticated requests, IP for anonymous
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userId ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
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
