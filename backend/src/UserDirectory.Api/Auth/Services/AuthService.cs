using UserDirectory.Api.Auth.Interfaces;
using UserDirectory.Api.Contracts;

namespace UserDirectory.Api.Auth.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserCredentialVerifier _credentialVerifier;
    private readonly IJwtTokenFactory _jwtTokenFactory;

    public AuthService(IUserCredentialVerifier credentialVerifier, IJwtTokenFactory jwtTokenFactory)
    {
        _credentialVerifier = credentialVerifier;
        _jwtTokenFactory = jwtTokenFactory;
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var authenticatedUser = await _credentialVerifier.VerifyAsync(email, password, cancellationToken);
        if (authenticatedUser is null)
        {
            return null;
        }

        return _jwtTokenFactory.CreateToken(authenticatedUser);
    }
}
