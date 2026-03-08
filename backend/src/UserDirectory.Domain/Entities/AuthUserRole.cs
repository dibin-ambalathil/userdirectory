namespace UserDirectory.Domain.Entities;

public class AuthUserRole
{
    private AuthUserRole()
    {
        // Required by EF Core.
    }

    public AuthUserRole(Guid userId, Guid roleId, DateTime? assignedAt = null)
    {
        UserId = userId;
        RoleId = roleId;
        AssignedAt = assignedAt ?? DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }

    public Guid RoleId { get; private set; }

    public DateTime AssignedAt { get; private set; }

    public AuthUser User { get; private set; } = null!;

    public AuthRole Role { get; private set; } = null!;
}
