using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserDirectory.Domain.Entities;

namespace UserDirectory.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Age)
            .IsRequired();

        builder.Property(u => u.City)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.State)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Pincode)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .IsRequired();
    }
}
