using Microsoft.EntityFrameworkCore;
using UserDirectory.Api.Auth.Interfaces;
using UserDirectory.Api.Auth.Models;
using UserDirectory.Infrastructure.Persistence;
using UserDirectory.Infrastructure.Security;

namespace UserDirectory.Api.Auth.Services;

public sealed class DatabaseUserCredentialVerifier : IUserCredentialVerifier
{
    private readonly UserDirectoryDbContext _dbContext;
    private readonly AuthOptions _authOptions;

    public DatabaseUserCredentialVerifier(UserDirectoryDbContext dbContext, AuthOptions authOptions)
    {
        _dbContext = dbContext;
        _authOptions = authOptions;
    }

    public async Task<AuthenticatedUser?> VerifyAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = (email ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalizedEmail) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var user = await _dbContext.AuthUsers
            .Include(authUser => authUser.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(
                authUser => authUser.EmailNormalized == normalizedEmail,
                cancellationToken);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        if (user.IsLockedOut())
        {
            return null;
        }

        if (!PasswordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
        {
            user.RegisterFailedLoginAttempt(_authOptions.MaxFailedLoginAttempts, _authOptions.LockoutMinutes);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return null;
        }

        user.MarkLogin();
        await _dbContext.SaveChangesAsync(cancellationToken);

        var roles = user.UserRoles
            .Select(userRole => userRole.Role.Name)
            .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new AuthenticatedUser(user.Id, user.Email, roles);
    }
}
