using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using UserDirectory.Api.Contracts;

namespace UserDirectory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private const string HardcodedEmail = "test@mail.com";
    private const string HardcodedPassword = "Qwer@4321";

    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var email = request.Email?.Trim() ?? string.Empty;
        var password = request.Password ?? string.Empty;

        if (!IsValidCredential(email, password))
        {
            return Unauthorized(new ErrorResponse("Invalid email or password."));
        }

        return Ok(CreateLoginResponse(email));
    }

    private static bool IsValidCredential(string email, string password)
    {
        return string.Equals(email, HardcodedEmail, StringComparison.OrdinalIgnoreCase)
            && string.Equals(password, HardcodedPassword, StringComparison.Ordinal);
    }

    private LoginResponse CreateLoginResponse(string email)
    {
        var issuer = _configuration["Auth:Issuer"] ?? "UserDirectory.Api";
        var audience = _configuration["Auth:Audience"] ?? "user-directory-api";
        var jwtKey = _configuration["Auth:LocalJwtKey"]
            ?? throw new InvalidOperationException("Auth:LocalJwtKey configuration is required.");
        var expirationMinutes = Math.Max(_configuration.GetValue("Auth:TokenExpirationMinutes", 120), 1);

        var now = DateTime.UtcNow;
        var expiresAtUtc = now.AddMinutes(expirationMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Name, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: now,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
        return new LoginResponse(accessToken, expiresAtUtc);
    }
}
