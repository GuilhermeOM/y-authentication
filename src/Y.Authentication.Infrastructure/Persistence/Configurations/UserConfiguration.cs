using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Y.Authentication.Domain.Aggregates.User;

namespace Y.Authentication.Infrastructure.Persistence.Configurations;
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(prop => prop.Email).IsRequired();
        builder.Property(prop => prop.VerificationToken).IsRequired();

        builder.HasIndex(prop => prop.Email)
            .IsUnique();

        builder.HasIndex(prop => prop.VerificationToken)
            .IsUnique();

        builder.Property(prop => prop.Email)
            .IsRequired()
            .HasMaxLength(256);
    }
}
