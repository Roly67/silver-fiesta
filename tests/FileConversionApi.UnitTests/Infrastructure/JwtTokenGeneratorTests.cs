// <copyright file="JwtTokenGeneratorTests.cs" company="FileConversionApi">
// FileConversionApi
// </copyright>

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using FileConversionApi.Domain.Entities;
using FileConversionApi.Infrastructure.Options;
using FileConversionApi.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FileConversionApi.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for the <see cref="JwtTokenGenerator"/> class.
/// </summary>
public class JwtTokenGeneratorTests
{
    private const string TestSecret = "ThisIsAVeryLongSecretKeyForTestingPurposes123456789!";
    private const string TestIssuer = "TestIssuer";
    private const string TestAudience = "TestAudience";
    private const int TestExpirationMinutes = 30;

    private readonly JwtSettings defaultSettings;
    private readonly IOptions<JwtSettings> defaultOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenGeneratorTests"/> class.
    /// </summary>
    public JwtTokenGeneratorTests()
    {
        this.defaultSettings = new JwtSettings
        {
            Secret = TestSecret,
            Issuer = TestIssuer,
            Audience = TestAudience,
            TokenExpirationMinutes = TestExpirationMinutes,
        };
        this.defaultOptions = Options.Create(this.defaultSettings);
    }

    /// <summary>
    /// Tests that the constructor throws ArgumentNullException when settings is null.
    /// </summary>
    [Fact]
    public void Constructor_WhenSettingsIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        IOptions<JwtSettings>? nullOptions = null;

        // Act
        var act = () => new JwtTokenGenerator(nullOptions!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("settings");
    }

    /// <summary>
    /// Tests that GenerateToken returns a non-empty access token.
    /// </summary>
    [Fact]
    public void GenerateToken_WhenCalled_ReturnsNonEmptyAccessToken()
    {
        // Arrange
        var generator = new JwtTokenGenerator(this.defaultOptions);
        var user = User.Create("test@example.com", "hashedPassword");

        // Act
        var result = generator.GenerateToken(user);

        // Assert
        result.AccessToken.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Tests that GenerateToken returns a non-empty refresh token.
    /// </summary>
    [Fact]
    public void GenerateToken_WhenCalled_ReturnsNonEmptyRefreshToken()
    {
        // Arrange
        var generator = new JwtTokenGenerator(this.defaultOptions);
        var user = User.Create("test@example.com", "hashedPassword");

        // Act
        var result = generator.GenerateToken(user);

        // Assert
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Tests that GenerateToken returns correct expiration time in seconds.
    /// </summary>
    [Fact]
    public void GenerateToken_WhenCalled_ReturnsCorrectExpiresIn()
    {
        // Arrange
        var generator = new JwtTokenGenerator(this.defaultOptions);
        var user = User.Create("test@example.com", "hashedPassword");
        var expectedExpiresIn = TestExpirationMinutes * 60;

        // Act
        var result = generator.GenerateToken(user);

        // Assert
        result.ExpiresIn.Should().Be(expectedExpiresIn);
    }

    /// <summary>
    /// Tests that the generated token contains the correct subject claim.
    /// </summary>
    [Fact]
    public void GenerateToken_WhenCalled_TokenContainsCorrectSubjectClaim()
    {
        // Arrange
        var generator = new JwtTokenGenerator(this.defaultOptions);
        var user = User.Create("test@example.com", "hashedPassword");

        // Act
        var result = generator.GenerateToken(user);
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.AccessToken);

        // Assert
        token.Claims.Should().Contain(c =>
            c.Type == JwtRegisteredClaimNames.Sub &&
            c.Value == user.Id.Value.ToString());
    }

    /// <summary>
    /// Tests that the generated token contains the correct email claim.
    /// </summary>
    [Fact]
    public void GenerateToken_WhenCalled_TokenContainsCorrectEmailClaim()
    {
        // Arrange
        var generator = new JwtTokenGenerator(this.defaultOptions);
        var userEmail = "test@example.com";
        var user = User.Create(userEmail, "hashedPassword");

        // Act
        var result = generator.GenerateToken(user);
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.AccessToken);

        // Assert
        token.Claims.Should().Contain(c =>
            c.Type == JwtRegisteredClaimNames.Email &&
            c.Value == userEmail);
    }

    /// <summary>
    /// Tests that the generated token contains a JTI claim.
    /// </summary>
    [Fact]
    public void GenerateToken_WhenCalled_TokenContainsJtiClaim()
    {
        // Arrange
        var generator = new JwtTokenGenerator(this.defaultOptions);
        var user = User.Create("test@example.com", "hashedPassword");

        // Act
        var result = generator.GenerateToken(user);
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.AccessToken);

        // Assert
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
    }

    /// <summary>
    /// Tests that the generated token has the correct issuer.
    /// </summary>
    [Fact]
    public void GenerateToken_WhenCalled_TokenHasCorrectIssuer()
    {
        // Arrange
        var generator = new JwtTokenGenerator(this.defaultOptions);
        var user = User.Create("test@example.com", "hashedPassword");

        // Act
        var result = generator.GenerateToken(user);
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.AccessToken);

        // Assert
        token.Issuer.Should().Be(TestIssuer);
    }

    /// <summary>
    /// Tests that the generated token has the correct audience.
    /// </summary>
    [Fact]
    public void GenerateToken_WhenCalled_TokenHasCorrectAudience()
    {
        // Arrange
        var generator = new JwtTokenGenerator(this.defaultOptions);
        var user = User.Create("test@example.com", "hashedPassword");

        // Act
        var result = generator.GenerateToken(user);
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.AccessToken);

        // Assert
        token.Audiences.Should().Contain(TestAudience);
    }

    /// <summary>
    /// Tests that the generated token has the correct expiration time.
    /// </summary>
    [Fact]
    public void GenerateToken_WhenCalled_TokenHasCorrectExpiration()
    {
        // Arrange
        var generator = new JwtTokenGenerator(this.defaultOptions);
        var user = User.Create("test@example.com", "hashedPassword");

        // JWT tokens have second-level precision, so we truncate to seconds for comparison
        var beforeGeneration = new DateTime(
            DateTime.UtcNow.Year,
            DateTime.UtcNow.Month,
            DateTime.UtcNow.Day,
            DateTime.UtcNow.Hour,
            DateTime.UtcNow.Minute,
            DateTime.UtcNow.Second,
            DateTimeKind.Utc);

        // Act
        var result = generator.GenerateToken(user);
        var afterGeneration = DateTime.UtcNow;

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result.AccessToken);

        // Assert
        var expectedMinExpiration = beforeGeneration.AddMinutes(TestExpirationMinutes);
        var expectedMaxExpiration = afterGeneration.AddMinutes(TestExpirationMinutes).AddSeconds(1);

        token.ValidTo.Should().BeOnOrAfter(expectedMinExpiration);
        token.ValidTo.Should().BeOnOrBefore(expectedMaxExpiration);
    }

    /// <summary>
    /// Tests that the generated token can be validated with the correct key.
    /// </summary>
    [Fact]
    public void GenerateToken_WhenCalled_TokenCanBeValidated()
    {
        // Arrange
        var generator = new JwtTokenGenerator(this.defaultOptions);
        var user = User.Create("test@example.com", "hashedPassword");

        // Act
        var result = generator.GenerateToken(user);

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = TestIssuer,
            ValidAudience = TestAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecret)),
            ClockSkew = TimeSpan.Zero,
        };

        // Assert
        var act = () => tokenHandler.ValidateToken(result.AccessToken, validationParameters, out _);
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that each generated token has a unique JTI.
    /// </summary>
    [Fact]
    public void GenerateToken_WhenCalledMultipleTimes_EachTokenHasUniqueJti()
    {
        // Arrange
        var generator = new JwtTokenGenerator(this.defaultOptions);
        var user = User.Create("test@example.com", "hashedPassword");
        var tokenHandler = new JwtSecurityTokenHandler();

        // Act
        var result1 = generator.GenerateToken(user);
        var result2 = generator.GenerateToken(user);

        var token1 = tokenHandler.ReadJwtToken(result1.AccessToken);
        var token2 = tokenHandler.ReadJwtToken(result2.AccessToken);

        var jti1 = token1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = token2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        // Assert
        jti1.Should().NotBe(jti2);
    }

    /// <summary>
    /// Tests that each call generates a unique refresh token.
    /// </summary>
    [Fact]
    public void GenerateToken_WhenCalledMultipleTimes_EachRefreshTokenIsUnique()
    {
        // Arrange
        var generator = new JwtTokenGenerator(this.defaultOptions);
        var user = User.Create("test@example.com", "hashedPassword");

        // Act
        var result1 = generator.GenerateToken(user);
        var result2 = generator.GenerateToken(user);

        // Assert
        result1.RefreshToken.Should().NotBe(result2.RefreshToken);
    }

    /// <summary>
    /// Tests that RefreshToken generates a new token pair.
    /// </summary>
    [Fact]
    public void RefreshToken_WhenCalled_ReturnsNewTokenPair()
    {
        // Arrange
        var generator = new JwtTokenGenerator(this.defaultOptions);
        var user = User.Create("test@example.com", "hashedPassword");
        var originalToken = generator.GenerateToken(user);

        // Act
        var refreshedToken = generator.RefreshToken(originalToken.RefreshToken, user);

        // Assert
        refreshedToken.AccessToken.Should().NotBeNullOrEmpty();
        refreshedToken.RefreshToken.Should().NotBeNullOrEmpty();
        refreshedToken.AccessToken.Should().NotBe(originalToken.AccessToken);
        refreshedToken.RefreshToken.Should().NotBe(originalToken.RefreshToken);
    }

    /// <summary>
    /// Tests that RefreshToken returns correct expiration time.
    /// </summary>
    [Fact]
    public void RefreshToken_WhenCalled_ReturnsCorrectExpiresIn()
    {
        // Arrange
        var generator = new JwtTokenGenerator(this.defaultOptions);
        var user = User.Create("test@example.com", "hashedPassword");
        var originalToken = generator.GenerateToken(user);
        var expectedExpiresIn = TestExpirationMinutes * 60;

        // Act
        var refreshedToken = generator.RefreshToken(originalToken.RefreshToken, user);

        // Assert
        refreshedToken.ExpiresIn.Should().Be(expectedExpiresIn);
    }

    /// <summary>
    /// Tests that the refresh token has correct length for base64 encoded 64 bytes.
    /// </summary>
    [Fact]
    public void GenerateToken_WhenCalled_RefreshTokenHasExpectedLength()
    {
        // Arrange
        var generator = new JwtTokenGenerator(this.defaultOptions);
        var user = User.Create("test@example.com", "hashedPassword");

        // Act
        var result = generator.GenerateToken(user);

        // Assert - Base64 encoding of 64 bytes results in 88 characters
        result.RefreshToken.Length.Should().Be(88);
    }

    /// <summary>
    /// Tests that token generation works with different expiration settings.
    /// </summary>
    /// <param name="expirationMinutes">The token expiration time in minutes.</param>
    [Theory]
    [InlineData(1)]
    [InlineData(60)]
    [InlineData(1440)]
    public void GenerateToken_WithDifferentExpirations_ReturnsCorrectExpiresIn(int expirationMinutes)
    {
        // Arrange
        var settings = new JwtSettings
        {
            Secret = TestSecret,
            Issuer = TestIssuer,
            Audience = TestAudience,
            TokenExpirationMinutes = expirationMinutes,
        };
        var options = Options.Create(settings);
        var generator = new JwtTokenGenerator(options);
        var user = User.Create("test@example.com", "hashedPassword");

        // Act
        var result = generator.GenerateToken(user);

        // Assert
        result.ExpiresIn.Should().Be(expirationMinutes * 60);
    }

    /// <summary>
    /// Tests that TokenType is set to Bearer.
    /// </summary>
    [Fact]
    public void GenerateToken_WhenCalled_TokenTypeIsBearer()
    {
        // Arrange
        var generator = new JwtTokenGenerator(this.defaultOptions);
        var user = User.Create("test@example.com", "hashedPassword");

        // Act
        var result = generator.GenerateToken(user);

        // Assert
        result.TokenType.Should().Be("Bearer");
    }
}
