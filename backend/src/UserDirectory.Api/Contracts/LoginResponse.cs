namespace UserDirectory.Api.Contracts;

public sealed record LoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc);
