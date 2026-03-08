using UserDirectory.Domain.Entities;

namespace UserDirectory.Api.Tests.Auth;

public sealed class AuthUserTests
{
    [Fact]
    public void Constructor_WhenNameMissing_Throws()
    {
        var (hash, salt) = ("hash", "salt");

        Assert.Throws<ArgumentException>(() =>
            new AuthUser(Guid.NewGuid(), "test@mail.com", hash, salt, firstName: "", lastName: "User"));
    }

    [Fact]
    public void RegisterFailedLoginAttempt_WhenThresholdReached_SetsLockout()
    {
        var user = new AuthUser(Guid.NewGuid(), "test@mail.com", "hash", "salt", "Test", "User");

        user.RegisterFailedLoginAttempt(maxFailedLoginAttempts: 2, lockoutMinutes: 10, utcNow: DateTime.UtcNow);
        user.RegisterFailedLoginAttempt(maxFailedLoginAttempts: 2, lockoutMinutes: 10, utcNow: DateTime.UtcNow);

        Assert.NotNull(user.LockoutEndAt);
        Assert.Equal(0, user.FailedLoginAttempts);
    }

    [Fact]
    public void MarkLogin_ClearsLockoutAndAttempts()
    {
        var now = DateTime.UtcNow;
        var user = new AuthUser(Guid.NewGuid(), "test@mail.com", "hash", "salt", "Test", "User");

        user.RegisterFailedLoginAttempt(maxFailedLoginAttempts: 1, lockoutMinutes: 10, utcNow: now);
        user.MarkLogin(now.AddMinutes(1));

        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockoutEndAt);
        Assert.NotNull(user.LastLoginAt);
    }
}
