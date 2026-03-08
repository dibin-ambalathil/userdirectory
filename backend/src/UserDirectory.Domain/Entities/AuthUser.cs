namespace UserDirectory.Domain.Entities;

public class AuthUser
{
    private AuthUser()
    {
        // Required by EF Core.
    }

    public AuthUser(
        Guid id,
        string email,
        string passwordHash,
        string passwordSalt,
        string? firstName = null,
        string? lastName = null,
        bool isActive = true,
        DateTime? createdAt = null)
    {
        Id = id;
        CreatedAt = createdAt ?? DateTime.UtcNow;
        IsActive = isActive;

        SetEmail(email);
        SetName(firstName, lastName);
        SetPassword(passwordHash, passwordSalt);
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string EmailNormalized { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string PasswordSalt { get; private set; } = string.Empty;

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public DateTime? LastLoginAt { get; private set; }

    public int FailedLoginAttempts { get; private set; }

    public DateTime? LockoutEndAt { get; private set; }

    public ICollection<AuthUserRole> UserRoles { get; private set; } = new List<AuthUserRole>();

    public void SetEmail(string email)
    {
        var normalized = NormalizeRequired(email, nameof(email));
        Email = normalized;
        EmailNormalized = normalized.ToUpperInvariant();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetName(string? firstName, string? lastName)
    {
        FirstName = NormalizeRequired(firstName, nameof(firstName));
        LastName = NormalizeRequired(lastName, nameof(lastName));
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPassword(string passwordHash, string passwordSalt)
    {
        PasswordHash = NormalizeRequired(passwordHash, nameof(passwordHash));
        PasswordSalt = NormalizeRequired(passwordSalt, nameof(passwordSalt));
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsLockedOut(DateTime? utcNow = null)
    {
        if (!LockoutEndAt.HasValue)
        {
            return false;
        }

        return LockoutEndAt.Value > (utcNow ?? DateTime.UtcNow);
    }

    public void RegisterFailedLoginAttempt(
        int maxFailedLoginAttempts,
        int lockoutMinutes,
        DateTime? utcNow = null)
    {
        if (maxFailedLoginAttempts < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxFailedLoginAttempts));
        }

        if (lockoutMinutes < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(lockoutMinutes));
        }

        var now = utcNow ?? DateTime.UtcNow;
        FailedLoginAttempts += 1;

        if (FailedLoginAttempts >= maxFailedLoginAttempts)
        {
            LockoutEndAt = now.AddMinutes(lockoutMinutes);
            FailedLoginAttempts = 0;
        }

        UpdatedAt = now;
    }

    public void MarkLogin(DateTime? loginTimeUtc = null)
    {
        var loginTime = loginTimeUtc ?? DateTime.UtcNow;
        LastLoginAt = loginTime;
        FailedLoginAttempts = 0;
        LockoutEndAt = null;
        UpdatedAt = loginTime;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string NormalizeRequired(string? value, string parameterName)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return normalized;
    }
}
