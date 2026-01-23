using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Y.Authentication.Domain.Aggregates.Role;

namespace Y.Authentication.Infrastructure.Persistence.Configurations;
internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasIndex(property => property.Name)
            .IsUnique();

        builder.Property(property => property.Name)
            .IsRequired()
            .HasMaxLength(50);
    }
}
