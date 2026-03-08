using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UserDirectory.Api.Auth.Interfaces;
using UserDirectory.Api.Auth.Models;
using UserDirectory.Api.Contracts;

namespace UserDirectory.Api.Auth.Services;

public sealed class JwtTokenFactory : IJwtTokenFactory
{
    private readonly AuthOptions _authOptions;

    public JwtTokenFactory(AuthOptions authOptions)
    {
        _authOptions = authOptions;
    }

    public LoginResponse CreateToken(AuthenticatedUser user)
    {
        var now = DateTime.UtcNow;
        var expiresAtUtc = now.AddMinutes(Math.Max(_authOptions.TokenExpirationMinutes, 1));

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.LocalJwtKey)),
            SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _authOptions.Issuer,
            audience: _authOptions.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
        return new LoginResponse(accessToken, expiresAtUtc);
    }
}
