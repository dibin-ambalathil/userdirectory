using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserDirectory.Api.Auth;
using UserDirectory.Api.Auth.Models;
using UserDirectory.Api.Auth.Services;

namespace UserDirectory.Api.Tests.Auth;

public sealed class JwtTokenFactoryTests
{
    [Fact]
    public void CreateToken_IncludesIdentityAndRoleClaims()
    {
        var options = new AuthOptions
        {
            Issuer = "issuer-test",
            Audience = "audience-test",
            LocalJwtKey = new string('x', 64),
            TokenExpirationMinutes = 30
        };

        var userId = Guid.NewGuid();
        var user = new AuthenticatedUser(userId, "test@mail.com", new[] { "Admin", "User" });

        var factory = new JwtTokenFactory(options);

        var response = factory.CreateToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.AccessToken);

        Assert.Equal("issuer-test", jwt.Issuer);
        Assert.Contains("audience-test", jwt.Audiences);
        Assert.Contains(jwt.Claims, claim => claim.Type == JwtRegisteredClaimNames.Email && claim.Value == "test@mail.com");
        Assert.Contains(jwt.Claims, claim => claim.Type == ClaimTypes.NameIdentifier && claim.Value == userId.ToString());
        Assert.Contains(jwt.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == "Admin");
        Assert.Contains(jwt.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == "User");
        Assert.True(response.ExpiresAtUtc > DateTime.UtcNow.AddMinutes(25));
    }
}
