namespace UserDirectory.Domain.Entities;

public class AuthRole
{
    private AuthRole()
    {
        // Required by EF Core.
    }

    public AuthRole(
        Guid id,
        string name,
        string? description = null,
        DateTime? createdAt = null)
    {
        Id = id;
        CreatedAt = createdAt ?? DateTime.UtcNow;
        SetName(name);
        SetDescription(description);
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public ICollection<AuthUserRole> UserRoles { get; private set; } = new List<AuthUserRole>();

    public void SetName(string name)
    {
        var normalized = name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Role name cannot be empty.", nameof(name));
        }

        Name = normalized;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDescription(string? description)
    {
        Description = (description ?? string.Empty).Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
