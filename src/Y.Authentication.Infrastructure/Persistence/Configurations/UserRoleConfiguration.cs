using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Y.Authentication.Domain.Entities;

namespace Y.Authentication.Infrastructure.Persistence.Configurations;
internal sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder
            .HasOne(property => property.User)
            .WithMany(property => property.Roles)
            .IsRequired();

        builder.Property(property => property.UserId).IsRequired();
        builder.Property(property => property.RoleId).IsRequired();

        builder.HasIndex(property => property.UserId).IsUnique(false);
    }
}
