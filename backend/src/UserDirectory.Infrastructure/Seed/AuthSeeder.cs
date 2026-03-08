using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using UserDirectory.Domain.Entities;
using UserDirectory.Infrastructure.Persistence;
using UserDirectory.Infrastructure.Security;

namespace UserDirectory.Infrastructure.Seed;

public static class AuthSeeder
{
    private const string SeedEmail = "test@mail.com";
    private const string SeedPassword = "Qwer@4321";
    private const string SeedFirstName = "Test";
    private const string SeedLastName = "User";
    private const string SeedUserEmailKey = "Auth:SeedUserEmail";
    private const string SeedUserPasswordKey = "Auth:SeedUserPassword";
    private const string SeedUserFirstNameKey = "Auth:SeedUserFirstName";
    private const string SeedUserLastNameKey = "Auth:SeedUserLastName";
    private const string SeedUserIsAdminKey = "Auth:SeedUserIsAdmin";

    public static async Task SeedAsync(
        UserDirectoryDbContext dbContext,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        CancellationToken cancellationToken = default)
    {
        var adminRole = await dbContext.AuthRoles
            .FirstOrDefaultAsync(role => role.Name == "Admin", cancellationToken);

        if (adminRole is null)
        {
            adminRole = new AuthRole(Guid.NewGuid(), "Admin", "Administrator with full access");
            await dbContext.AuthRoles.AddAsync(adminRole, cancellationToken);
        }

        var userRole = await dbContext.AuthRoles
            .FirstOrDefaultAsync(role => role.Name == "User", cancellationToken);

        if (userRole is null)
        {
            userRole = new AuthRole(Guid.NewGuid(), "User", "Standard authenticated user");
            await dbContext.AuthRoles.AddAsync(userRole, cancellationToken);
        }

        var seedUser = ResolveSeedUser(configuration, hostEnvironment);
        if (seedUser is null)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var emailNormalized = seedUser.Email.ToUpperInvariant();

        var authUser = await dbContext.AuthUsers
            .Include(user => user.UserRoles)
            .FirstOrDefaultAsync(user => user.EmailNormalized == emailNormalized, cancellationToken);

        if (authUser is null)
        {
            var (passwordHash, passwordSalt) = PasswordHasher.HashPassword(seedUser.Password);

            authUser = new AuthUser(
                Guid.NewGuid(),
                seedUser.Email,
                passwordHash,
                passwordSalt,
                firstName: seedUser.FirstName,
                lastName: seedUser.LastName,
                isActive: true);

            await dbContext.AuthUsers.AddAsync(authUser, cancellationToken);
        }

        var userRoles = authUser.UserRoles;

        if (userRoles.All(assignment => assignment.RoleId != userRole.Id))
        {
            userRoles.Add(new AuthUserRole(authUser.Id, userRole.Id));
        }

        var shouldAssignAdminRole = hostEnvironment.IsDevelopment() || IsSeedUserAdmin(configuration);
        if (shouldAssignAdminRole && userRoles.All(assignment => assignment.RoleId != adminRole.Id))
        {
            userRoles.Add(new AuthUserRole(authUser.Id, adminRole.Id));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static SeedUserDefinition? ResolveSeedUser(IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        if (hostEnvironment.IsDevelopment())
        {
            return new SeedUserDefinition(SeedEmail, SeedPassword, SeedFirstName, SeedLastName);
        }

        var email = configuration[SeedUserEmailKey]?.Trim();
        var password = configuration[SeedUserPasswordKey];
        var firstName = configuration[SeedUserFirstNameKey]?.Trim();
        var lastName = configuration[SeedUserLastNameKey]?.Trim();

        if (string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(firstName)
            || string.IsNullOrWhiteSpace(lastName))
        {
            return null;
        }

        return new SeedUserDefinition(email, password, firstName, lastName);
    }

    private static bool IsSeedUserAdmin(IConfiguration configuration)
    {
        return bool.TryParse(configuration[SeedUserIsAdminKey], out var isAdmin) && isAdmin;
    }

    private sealed record SeedUserDefinition(string Email, string Password, string FirstName, string LastName);
}
