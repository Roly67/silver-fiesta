// <copyright file="PasswordHasher.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Application.Interfaces;

namespace FileConversionApi.Infrastructure.Services;

/// <summary>
/// Implementation of password hashing using BCrypt.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    /// <inheritdoc/>
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }

    /// <inheritdoc/>
    public bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
