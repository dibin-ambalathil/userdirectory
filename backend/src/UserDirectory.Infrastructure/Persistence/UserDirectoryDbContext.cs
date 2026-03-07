using Microsoft.EntityFrameworkCore;
using UserDirectory.Domain.Entities;

namespace UserDirectory.Infrastructure.Persistence;

public sealed class UserDirectoryDbContext : DbContext
{
    public UserDirectoryDbContext(DbContextOptions<UserDirectoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserDirectoryDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
