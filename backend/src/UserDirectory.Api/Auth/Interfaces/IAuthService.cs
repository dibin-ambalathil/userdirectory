using UserDirectory.Api.Contracts;

namespace UserDirectory.Api.Auth.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
}
