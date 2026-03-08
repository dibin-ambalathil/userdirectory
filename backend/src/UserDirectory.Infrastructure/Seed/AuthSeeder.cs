using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using UserDirectory.Domain.Entities;
using UserDirectory.Infrastructure.Persistence;
using UserDirectory.Infrastructure.Security;

namespace UserDirectory.Infrastructure.Seed;

/// <summary>
/// Provides seeding functionality for authentication data including roles and users.
/// Seeding occurs during application startup via the InitializeDatabaseAsync method.
/// </summary>
public static class AuthSeeder
{
    // Default seed user credentials for development environment
    private const string SeedEmail = "test@mail.com";
    private const string SeedPassword = "Qwer@4321";
    private const string SeedFirstName = "Test";
    private const string SeedLastName = "User";

    // Default admin seed user credentials for development environment
    private const string SeedAdminEmail = "admin@mail.com";
    private const string SeedAdminPassword = "Admin@4321";
    private const string SeedAdminFirstName = "System";
    private const string SeedAdminLastName = "Admin";

    // Configuration keys for production seed users (must be set via environment variables)
    private const string SeedUserEmailKey = "Auth:SeedUserEmail";
    private const string SeedUserPasswordKey = "Auth:SeedUserPassword";
    private const string SeedUserFirstNameKey = "Auth:SeedUserFirstName";
    private const string SeedUserLastNameKey = "Auth:SeedUserLastName";
    private const string SeedUserIsAdminKey = "Auth:SeedUserIsAdmin";

    private const string SeedAdminEmailKey = "Auth:SeedAdminEmail";
    private const string SeedAdminPasswordKey = "Auth:SeedAdminPassword";
    private const string SeedAdminFirstNameKey = "Auth:SeedAdminFirstName";
    private const string SeedAdminLastNameKey = "Auth:SeedAdminLastName";

    /// <summary>
    /// Seeds authentication roles and users into the database.
    /// Creates Admin and User roles if they don't exist.
    /// Seeds default users in development, or configured users in production.
    /// </summary>
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
        if (seedUser is not null)
        {
            var authUser = await UpsertAuthUserAsync(dbContext, seedUser, cancellationToken);

            // Ensure seed user has User role
            if (authUser.UserRoles.All(assignment => assignment.RoleId != userRole.Id))
            {
                authUser.UserRoles.Add(new AuthUserRole(authUser.Id, userRole.Id));
            }

            // Assign Admin role in development, or in production if explicitly configured
            var shouldAssignAdminRole = hostEnvironment.IsDevelopment() || IsSeedUserAdmin(configuration);
            if (shouldAssignAdminRole && authUser.UserRoles.All(assignment => assignment.RoleId != adminRole.Id))
            {
                authUser.UserRoles.Add(new AuthUserRole(authUser.Id, adminRole.Id));
            }
        }

        var seedAdmin = ResolveSeedAdminUser(configuration, hostEnvironment);
        if (seedAdmin is not null)
        {
            var adminUser = await UpsertAuthUserAsync(dbContext, seedAdmin, cancellationToken);

            if (adminUser.UserRoles.All(assignment => assignment.RoleId != userRole.Id))
            {
                adminUser.UserRoles.Add(new AuthUserRole(adminUser.Id, userRole.Id));
            }

            if (adminUser.UserRoles.All(assignment => assignment.RoleId != adminRole.Id))
            {
                adminUser.UserRoles.Add(new AuthUserRole(adminUser.Id, adminRole.Id));
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<AuthUser> UpsertAuthUserAsync(
        UserDirectoryDbContext dbContext,
        SeedUserDefinition seedUser,
        CancellationToken cancellationToken)
    {
        var emailNormalized = seedUser.Email.ToUpperInvariant();

        var authUser = await dbContext.AuthUsers
            .Include(user => user.UserRoles)
            .FirstOrDefaultAsync(user => user.EmailNormalized == emailNormalized, cancellationToken);

        if (authUser is not null)
        {
            return authUser;
        }

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
        return authUser;
    }

    private static SeedUserDefinition? ResolveSeedUser(IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        // In development environment, always seed with default test user
        if (hostEnvironment.IsDevelopment())
        {
            return new SeedUserDefinition(SeedEmail, SeedPassword, SeedFirstName, SeedLastName);
        }

        // In production, only seed if all required configuration values are provided
        // This prevents accidental seeding in production without explicit configuration
        var email = configuration[SeedUserEmailKey]?.Trim();
        var password = configuration[SeedUserPasswordKey];
        var firstName = configuration[SeedUserFirstNameKey]?.Trim();
        var lastName = configuration[SeedUserLastNameKey]?.Trim();

        if (string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(firstName)
            || string.IsNullOrWhiteSpace(lastName))
        {
            return null; // No seeding in production without full config
        }

        return new SeedUserDefinition(email, password, firstName, lastName);
    }

    private static SeedUserDefinition? ResolveSeedAdminUser(IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        // In development environment, always seed with default admin user
        if (hostEnvironment.IsDevelopment())
        {
            return new SeedUserDefinition(SeedAdminEmail, SeedAdminPassword, SeedAdminFirstName, SeedAdminLastName);
        }

        // In production, only seed admin user if all required configuration values are provided
        var email = configuration[SeedAdminEmailKey]?.Trim();
        var password = configuration[SeedAdminPasswordKey];
        var firstName = configuration[SeedAdminFirstNameKey]?.Trim();
        var lastName = configuration[SeedAdminLastNameKey]?.Trim();

        if (string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(firstName)
            || string.IsNullOrWhiteSpace(lastName))
        {
            return null; // No seeding in production without full config
        }

        return new SeedUserDefinition(email, password, firstName, lastName);
    }

    private static bool IsSeedUserAdmin(IConfiguration configuration)
    {
        return bool.TryParse(configuration[SeedUserIsAdminKey], out var isAdmin) && isAdmin;
    }

    private sealed record SeedUserDefinition(string Email, string Password, string FirstName, string LastName);
}
