using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UserDirectory.Api.Auth;
using UserDirectory.Api.Auth.Services;
using UserDirectory.Domain.Entities;
using UserDirectory.Infrastructure.Persistence;
using UserDirectory.Infrastructure.Security;

namespace UserDirectory.Api.Tests.Auth;

public sealed class DatabaseUserCredentialVerifierTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<UserDirectoryDbContext> _dbOptions;

    public DatabaseUserCredentialVerifierTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        _dbOptions = new DbContextOptionsBuilder<UserDirectoryDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var dbContext = new UserDirectoryDbContext(_dbOptions);
        dbContext.Database.EnsureCreated();
    }

    [Fact]
    public async Task VerifyAsync_WithWrongPassword_TracksFailuresAndLocksUser()
    {
        var userId = await SeedUserAsync("test@mail.com", "Qwer@4321");
        var options = new AuthOptions
        {
            LocalJwtKey = new string('k', 64),
            MaxFailedLoginAttempts = 2,
            LockoutMinutes = 30
        };

        await using var dbContext = new UserDirectoryDbContext(_dbOptions);
        var verifier = new DatabaseUserCredentialVerifier(dbContext, options);

        var firstAttempt = await verifier.VerifyAsync("test@mail.com", "wrong", CancellationToken.None);
        var secondAttempt = await verifier.VerifyAsync("test@mail.com", "wrong", CancellationToken.None);
        var lockedOutAttempt = await verifier.VerifyAsync("test@mail.com", "Qwer@4321", CancellationToken.None);

        Assert.Null(firstAttempt);
        Assert.Null(secondAttempt);
        Assert.Null(lockedOutAttempt);

        var user = await dbContext.AuthUsers.SingleAsync(entity => entity.Id == userId);
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.NotNull(user.LockoutEndAt);
        Assert.True(user.LockoutEndAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task VerifyAsync_WithValidCredentials_ResetsLockoutAndSetsLastLogin()
    {
        var userId = await SeedUserAsync("test@mail.com", "Qwer@4321", markAsPreviouslyLocked: true);
        var options = new AuthOptions
        {
            LocalJwtKey = new string('k', 64),
            MaxFailedLoginAttempts = 5,
            LockoutMinutes = 15
        };

        await using var dbContext = new UserDirectoryDbContext(_dbOptions);
        var verifier = new DatabaseUserCredentialVerifier(dbContext, options);

        var authenticated = await verifier.VerifyAsync("test@mail.com", "Qwer@4321", CancellationToken.None);

        Assert.NotNull(authenticated);
        Assert.Equal("test@mail.com", authenticated.Email);
        Assert.Contains("User", authenticated.Roles);

        var user = await dbContext.AuthUsers.SingleAsync(entity => entity.Id == userId);
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEndAt);
        Assert.NotNull(user.LastLoginAt);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private async Task<Guid> SeedUserAsync(string email, string password, bool markAsPreviouslyLocked = false)
    {
        await using var dbContext = new UserDirectoryDbContext(_dbOptions);

        var role = new AuthRole(Guid.NewGuid(), "User", "Standard user role");
        var (hash, salt) = PasswordHasher.HashPassword(password);

        var user = new AuthUser(
            Guid.NewGuid(),
            email,
            hash,
            salt,
            firstName: "Test",
            lastName: "User",
            isActive: true);

        if (markAsPreviouslyLocked)
        {
            user.RegisterFailedLoginAttempt(maxFailedLoginAttempts: 1, lockoutMinutes: 1, utcNow: DateTime.UtcNow.AddMinutes(-5));
        }

        await dbContext.AuthRoles.AddAsync(role);
        await dbContext.AuthUsers.AddAsync(user);
        await dbContext.AuthUserRoles.AddAsync(new AuthUserRole(user.Id, role.Id));
        await dbContext.SaveChangesAsync();

        return user.Id;
    }
}
