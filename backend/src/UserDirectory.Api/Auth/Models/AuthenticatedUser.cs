namespace UserDirectory.Api.Auth.Models;

public sealed record AuthenticatedUser(
    Guid Id,
    string Email,
    IReadOnlyList<string> Roles);
