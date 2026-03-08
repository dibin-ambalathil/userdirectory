using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UserDirectory.Api.Auth;
using UserDirectory.Api.Auth.Interfaces;
using UserDirectory.Api.Contracts;

namespace UserDirectory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthRateLimitPolicies.Login)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest? request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new ErrorResponse("Email and password are required."));
        }

        var email = request.Email?.Trim() ?? string.Empty;
        var password = request.Password ?? string.Empty;

        var loginResponse = await _authService.LoginAsync(email, password, cancellationToken);
        if (loginResponse is null)
        {
            return Unauthorized(new ErrorResponse("Invalid email or password."));
        }

        return Ok(loginResponse);
    }
}
