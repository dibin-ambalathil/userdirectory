using UserDirectory.Api.Auth.Models;

namespace UserDirectory.Api.Auth.Interfaces;

public interface IUserCredentialVerifier
{
    Task<AuthenticatedUser?> VerifyAsync(string email, string password, CancellationToken cancellationToken = default);
}
