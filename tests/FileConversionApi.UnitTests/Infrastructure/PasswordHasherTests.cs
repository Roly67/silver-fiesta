// <copyright file="PasswordHasherTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using FileConversionApi.Infrastructure.Services;
using FluentAssertions;

namespace FileConversionApi.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for the <see cref="PasswordHasher"/> class.
/// </summary>
public class PasswordHasherTests
{
    private readonly PasswordHasher hasher;

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordHasherTests"/> class.
    /// </summary>
    public PasswordHasherTests()
    {
        this.hasher = new PasswordHasher();
    }

    /// <summary>
    /// Tests that Hash returns a non-empty string.
    /// </summary>
    [Fact]
    public void Hash_WhenCalled_ReturnsNonEmptyString()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var result = this.hasher.Hash(password);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Tests that Hash returns a different value than the original password.
    /// </summary>
    [Fact]
    public void Hash_WhenCalled_ReturnsDifferentValueThanOriginalPassword()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var result = this.hasher.Hash(password);

        // Assert
        result.Should().NotBe(password);
    }

    /// <summary>
    /// Tests that Hash returns a valid BCrypt hash format.
    /// </summary>
    [Fact]
    public void Hash_WhenCalled_ReturnsValidBCryptFormat()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var result = this.hasher.Hash(password);

        // Assert
        // BCrypt hashes start with $2a$, $2b$, or $2y$ followed by the cost factor
        result.Should().MatchRegex(@"^\$2[aby]\$\d{2}\$.{53}$");
    }

    /// <summary>
    /// Tests that Hash uses cost factor 12.
    /// </summary>
    [Fact]
    public void Hash_WhenCalled_UsesCostFactor12()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var result = this.hasher.Hash(password);

        // Assert
        result.Should().Contain("$12$");
    }

    /// <summary>
    /// Tests that hashing the same password multiple times produces different hashes.
    /// </summary>
    [Fact]
    public void Hash_WhenCalledMultipleTimes_ProducesDifferentHashes()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = this.hasher.Hash(password);
        var hash2 = this.hasher.Hash(password);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    /// <summary>
    /// Tests that Verify returns true for matching password and hash.
    /// </summary>
    [Fact]
    public void Verify_WhenPasswordMatchesHash_ReturnsTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = this.hasher.Hash(password);

        // Act
        var result = this.hasher.Verify(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Verify returns false for non-matching password.
    /// </summary>
    [Fact]
    public void Verify_WhenPasswordDoesNotMatchHash_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword456!";
        var hash = this.hasher.Hash(password);

        // Act
        var result = this.hasher.Verify(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Verify is case-sensitive for passwords.
    /// </summary>
    [Fact]
    public void Verify_WhenPasswordDiffersInCase_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "testpassword123!";
        var hash = this.hasher.Hash(password);

        // Act
        var result = this.hasher.Verify(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Verify works correctly with empty password when hashed.
    /// </summary>
    [Fact]
    public void Verify_WhenPasswordIsEmpty_WorksCorrectly()
    {
        // Arrange
        var password = string.Empty;
        var hash = this.hasher.Hash(password);

        // Act
        var result = this.hasher.Verify(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Verify returns false when empty password is verified against non-empty hash.
    /// </summary>
    [Fact]
    public void Verify_WhenEmptyPasswordVerifiedAgainstNonEmptyHash_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = this.hasher.Hash(password);

        // Act
        var result = this.hasher.Verify(string.Empty, hash);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that Hash works with very long passwords.
    /// </summary>
    [Fact]
    public void Hash_WhenPasswordIsVeryLong_WorksCorrectly()
    {
        // Arrange
        var password = new string('a', 1000);

        // Act
        var hash = this.hasher.Hash(password);
        var result = this.hasher.Verify(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Hash works with special characters.
    /// </summary>
    [Fact]
    public void Hash_WhenPasswordContainsSpecialCharacters_WorksCorrectly()
    {
        // Arrange
        var password = "P@$$w0rd!#$%^&*()_+-=[]{}|;':\",./<>?`~";

        // Act
        var hash = this.hasher.Hash(password);
        var result = this.hasher.Verify(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Hash works with unicode characters.
    /// </summary>
    [Fact]
    public void Hash_WhenPasswordContainsUnicodeCharacters_WorksCorrectly()
    {
        // Arrange
        var password = "Passw0rd\u00e9\u00e8\u00ea\u4e2d\u6587";

        // Act
        var hash = this.hasher.Hash(password);
        var result = this.hasher.Verify(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Hash works with whitespace in password.
    /// </summary>
    [Fact]
    public void Hash_WhenPasswordContainsWhitespace_WorksCorrectly()
    {
        // Arrange
        var password = "  Password with spaces  ";

        // Act
        var hash = this.hasher.Hash(password);
        var result = this.hasher.Verify(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Verify correctly distinguishes passwords with different whitespace.
    /// </summary>
    [Fact]
    public void Verify_WhenPasswordsDifferByWhitespace_ReturnsFalse()
    {
        // Arrange
        var password = "Password with spaces";
        var differentPassword = "Passwordwithspaces";
        var hash = this.hasher.Hash(password);

        // Act
        var result = this.hasher.Verify(differentPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that hashing produces consistent length output.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    [Theory]
    [InlineData("a")]
    [InlineData("password")]
    [InlineData("VeryLongPasswordWithManyCharacters123456789!")]
    public void Hash_WithDifferentLengthPasswords_ProducesConsistentLengthOutput(string password)
    {
        // Arrange & Act
        var hash = this.hasher.Hash(password);

        // Assert
        // BCrypt hashes are always 60 characters
        hash.Length.Should().Be(60);
    }

    /// <summary>
    /// Tests that Verify throws when hash format is invalid.
    /// </summary>
    [Fact]
    public void Verify_WhenHashFormatIsInvalid_ThrowsException()
    {
        // Arrange
        var password = "TestPassword123!";
        var invalidHash = "not-a-valid-hash";

        // Act
        var act = () => this.hasher.Verify(password, invalidHash);

        // Assert
        act.Should().Throw<BCrypt.Net.SaltParseException>();
    }

    /// <summary>
    /// Tests that Verify returns false for similar but different passwords.
    /// </summary>
    [Fact]
    public void Verify_WhenPasswordsAreSimilarButDifferent_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var similarPassword = "TestPassword123";
        var hash = this.hasher.Hash(password);

        // Act
        var result = this.hasher.Verify(similarPassword, hash);

        // Assert
        result.Should().BeFalse();
    }
}
