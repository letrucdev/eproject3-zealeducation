using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Infrastructure.Services;

namespace ZealEducation.Infrastructure.IntegrationTests.Services;

public class JwtTokenServiceTests
{
    private static JwtSettings BuildSettings() => new()
    {
        Issuer = "ZealEducation.Test",
        Audience = "ZealEducation.Test.Client",
        SecretKey = "this-is-a-strong-256-bit-secret-key-1234567890",
        ExpiryMinutes = 60
    };

    private static UserAccount BuildUser(UserRole role = UserRole.Counselor) => new()
    {
        Id = Guid.NewGuid(),
        Username = "alice",
        Email = "alice@example.com",
        FullName = "Alice",
        Phone = "0900",
        Role = role,
        IsActive = true,
        PasswordHash = "h"
    };

    [Fact]
    public void GenerateToken_includes_required_claims_and_expiry()
    {
        var settings = BuildSettings();
        var service = new JwtTokenService(Options.Create(settings));
        var user = BuildUser(UserRole.SystemAdmin);

        var result = service.GenerateToken(user);

        result.Token.Should().NotBeNullOrWhiteSpace();
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(settings.ExpiryMinutes), TimeSpan.FromSeconds(5));

        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);
        token.Issuer.Should().Be(settings.Issuer);
        token.Audiences.Should().Contain(settings.Audience);

        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        token.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "SystemAdmin");
    }

    [Fact]
    public void GeneratePasswordResetToken_uses_short_expiry_and_marks_purpose()
    {
        var settings = BuildSettings();
        var service = new JwtTokenService(Options.Create(settings));
        var user = BuildUser();

        var result = service.GeneratePasswordResetToken(user);

        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(5), TimeSpan.FromSeconds(5),
            because: "password reset tokens expire in 5 minutes regardless of JwtSettings.ExpiryMinutes");

        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);
        token.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
        // Purpose claim is added but Role claim should NOT be present in a reset token
        token.Claims.Should().NotContain(c => c.Type == ClaimTypes.Role);
    }
}
