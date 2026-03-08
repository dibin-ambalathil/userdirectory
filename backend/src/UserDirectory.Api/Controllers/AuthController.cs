using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UserDirectory.Api.Auth;
using UserDirectory.Api.Auth.Interfaces;
using UserDirectory.Api.Contracts;

namespace UserDirectory.Api.Controllers;

/// <summary>
/// Handles authentication-related API endpoints (login, token issuance).
/// All endpoints are rate-limited to prevent brute-force attacks.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Initializes a new instance of <see cref="AuthController"/>.
    /// </summary>
    /// <param name="authService">Service responsible for credential validation and token generation.</param>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Authenticates a user with email and password credentials.
    /// Returns a JWT token on success; 401 on invalid credentials.
    /// Rate-limited via the <see cref="AuthRateLimitPolicies.Login"/> policy
    /// to mitigate brute-force and credential-stuffing attacks.
    /// </summary>
    /// <param name="request">Login payload containing email and password.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// <see cref="LoginResponse"/> with JWT token on success;
    /// 400 if the request body is missing;
    /// 401 if credentials are invalid.
    /// </returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthRateLimitPolicies.Login)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest? request, CancellationToken cancellationToken)
    {
        // Reject null/missing request body early
        if (request is null)
        {
            return BadRequest(new ErrorResponse("Email and password are required."));
        }

        // Normalize email and ensure password is never null
        var email = request.Email?.Trim() ?? string.Empty;
        var password = request.Password ?? string.Empty;

        // Delegate credential validation and token generation to the auth service
        var loginResponse = await _authService.LoginAsync(email, password, cancellationToken);

        // Return 401 if credentials are invalid (user not found, wrong password, or account locked)
        if (loginResponse is null)
        {
            return Unauthorized(new ErrorResponse("Invalid email or password."));
        }

        // Return JWT token and user details on successful authentication
        return Ok(loginResponse);
    }
}
