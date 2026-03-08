using UserDirectory.Api.Auth.Models;
using UserDirectory.Api.Contracts;

namespace UserDirectory.Api.Auth.Interfaces;

public interface IJwtTokenFactory
{
    LoginResponse CreateToken(AuthenticatedUser user);
}
