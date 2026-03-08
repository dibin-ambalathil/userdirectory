namespace UserDirectory.Api.Auth;

public sealed class AuthOptions
{
    public string Issuer { get; set; } = "UserDirectory.Api";

    public string Audience { get; set; } = "user-directory-api";

    public string LocalJwtKey { get; set; } = string.Empty;

    public int TokenExpirationMinutes { get; set; } = 120;

    public int MaxFailedLoginAttempts { get; set; } = 5;

    public int LockoutMinutes { get; set; } = 15;

    public int LoginRateLimitPerMinute { get; set; } = 10;

    public bool RequireHttpsMetadata { get; set; } = true;
}
