namespace UserDirectory.Application.Features.Users.DTOs;

public sealed record CreateUserRequest
{
    public string Name { get; init; } = string.Empty;

    public int Age { get; init; }

    public string City { get; init; } = string.Empty;

    public string State { get; init; } = string.Empty;

    public string Pincode { get; init; } = string.Empty;
}
