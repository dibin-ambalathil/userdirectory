using Microsoft.EntityFrameworkCore;
using UserDirectory.Domain.Entities;
using UserDirectory.Infrastructure.Persistence;

namespace UserDirectory.Infrastructure.Seed;

public static class UserSeeder
{
    public static async Task SeedAsync(UserDirectoryDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var seededUsers = new[]
        {
            new User(Guid.NewGuid(), "Aarav Menon", 29, "Bengaluru", "Karnataka", "560001"),
            new User(Guid.NewGuid(), "Priya Sharma", 34, "Pune", "Maharashtra", "411001"),
            new User(Guid.NewGuid(), "Nikhil Das", 24, "Kochi", "Kerala", "682001")
        };

        await dbContext.Users.AddRangeAsync(seededUsers, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
