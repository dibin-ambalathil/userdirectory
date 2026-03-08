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

    // Configuration keys for non-development seed admin
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
        // Ensure the "Admin" role exists
        var adminRole = await dbContext.AuthRoles
            .FirstOrDefaultAsync(role => role.Name == "Admin", cancellationToken);

        if (adminRole is null)
        {
            adminRole = new AuthRole(Guid.NewGuid(), "Admin", "Administrator with full access");
            await dbContext.AuthRoles.AddAsync(adminRole, cancellationToken);
        }

        // Ensure the "User" role exists
        var userRole = await dbContext.AuthRoles
            .FirstOrDefaultAsync(role => role.Name == "User", cancellationToken);

        if (userRole is null)
        {
            userRole = new AuthRole(Guid.NewGuid(), "User", "Standard authenticated user");
            await dbContext.AuthRoles.AddAsync(userRole, cancellationToken);
        }

        // Seed the standard test user (always in Development; config-driven otherwise)
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

        // Seed the dedicated admin user (always in Development; config-driven otherwise)
        var seedAdmin = ResolveSeedAdminUser(configuration, hostEnvironment);
        if (seedAdmin is not null)
        {
            var adminUser = await UpsertAuthUserAsync(dbContext, seedAdmin, cancellationToken);

            // Assign "User" role if not already assigned
            if (adminUser.UserRoles.All(assignment => assignment.RoleId != userRole.Id))
            {
                adminUser.UserRoles.Add(new AuthUserRole(adminUser.Id, userRole.Id));
            }

            // Admin user always gets the "Admin" role
            if (adminUser.UserRoles.All(assignment => assignment.RoleId != adminRole.Id))
            {
                adminUser.UserRoles.Add(new AuthUserRole(adminUser.Id, adminRole.Id));
            }
        }

        // Persist all role/user changes in a single transaction
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Inserts a new <see cref="AuthUser"/> if one with the same normalized email does not exist.
    /// Password is hashed using the configured <see cref="PasswordHasher"/> before storage.
    /// Returns the existing user if already present (no password update on re-seed).
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="seedUser">The seed user definition containing credentials and profile.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The existing or newly created <see cref="AuthUser"/>.</returns>
    private static async Task<AuthUser> UpsertAuthUserAsync(
        UserDirectoryDbContext dbContext,
        SeedUserDefinition seedUser,
        CancellationToken cancellationToken)
    {
        var emailNormalized = seedUser.Email.ToUpperInvariant();

        // Check if user already exists by normalized email
        var authUser = await dbContext.AuthUsers
            .Include(user => user.UserRoles)
            .FirstOrDefaultAsync(user => user.EmailNormalized == emailNormalized, cancellationToken);

        if (authUser is not null)
        {
            // User already seeded; skip to avoid overwriting password or duplicating
            return authUser;
        }

        // Hash the plain-text seed password before persisting
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

    /// <summary>
    /// Resolves the standard seed user definition.
    /// In Development, returns hardcoded test credentials.
    /// In other environments, reads from configuration keys; returns null if any key is missing.
    /// </summary>
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

        // All four fields are required; skip seeding if any is missing
        if (string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(firstName)
            || string.IsNullOrWhiteSpace(lastName))
        {
            return null; // No seeding in production without full config
        }

        return new SeedUserDefinition(email, password, firstName, lastName);
    }

    /// <summary>
    /// Resolves the admin seed user definition.
    /// In Development, returns hardcoded admin credentials.
    /// In other environments, reads from configuration keys; returns null if any key is missing.
    /// </summary>
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

        // All four fields are required; skip seeding if any is missing
        if (string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(firstName)
            || string.IsNullOrWhiteSpace(lastName))
        {
            return null; // No seeding in production without full config
        }

        return new SeedUserDefinition(email, password, firstName, lastName);
    }

    /// <summary>
    /// Checks whether the seed user should be granted the Admin role via configuration.
    /// Reads the "Auth:SeedUserIsAdmin" key; defaults to false if missing or unparseable.
    /// </summary>
    private static bool IsSeedUserAdmin(IConfiguration configuration)
    {
        return bool.TryParse(configuration[SeedUserIsAdminKey], out var isAdmin) && isAdmin;
    }

    /// <summary>
    /// Immutable record representing a seed user's credentials and profile data.
    /// </summary>
    private sealed record SeedUserDefinition(string Email, string Password, string FirstName, string LastName);
}
