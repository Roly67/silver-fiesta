// <copyright file="User.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Domain.ValueObjects;

namespace FileConversionApi.Domain.Entities;

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User
{
    private readonly List<ConversionJob> conversionJobs = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// Required for EF Core.
    /// </summary>
    private User()
    {
    }

    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public UserId Id { get; private set; }

    /// <summary>
    /// Gets the email address.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the password hash.
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the API key.
    /// </summary>
    public string ApiKey { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the date and time when the user was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the user is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the conversion jobs for this user.
    /// </summary>
    public IReadOnlyList<ConversionJob> ConversionJobs => this.conversionJobs.AsReadOnly();

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <param name="passwordHash">The password hash.</param>
    /// <returns>A new <see cref="User"/>.</returns>
    public static User Create(string email, string passwordHash)
    {
        return new User
        {
            Id = UserId.New(),
            Email = email,
            PasswordHash = passwordHash,
            ApiKey = GenerateApiKey(),
            CreatedAt = DateTimeOffset.UtcNow,
            IsActive = true,
        };
    }

    /// <summary>
    /// Updates the password hash.
    /// </summary>
    /// <param name="passwordHash">The new password hash.</param>
    public void UpdatePassword(string passwordHash)
    {
        this.PasswordHash = passwordHash;
    }

    /// <summary>
    /// Regenerates the API key.
    /// </summary>
    /// <returns>The new API key.</returns>
    public string RegenerateApiKey()
    {
        this.ApiKey = GenerateApiKey();
        return this.ApiKey;
    }

    /// <summary>
    /// Deactivates the user.
    /// </summary>
    public void Deactivate()
    {
        this.IsActive = false;
    }

    /// <summary>
    /// Activates the user.
    /// </summary>
    public void Activate()
    {
        this.IsActive = true;
    }

    private static string GenerateApiKey()
    {
        return $"fca_{Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("=", string.Empty).Replace("+", string.Empty).Replace("/", string.Empty)}";
    }
}
