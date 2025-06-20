using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Y.Authentication.Domain.Entities;

namespace Y.Authentication.Infrastructure.Persistence.Configurations;
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(prop => prop.Email)
            .IsUnique();

        builder.Property(prop => prop.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasOne(entity => entity.Metadata)
            .WithOne(entity => entity.User)
            .HasForeignKey<User>(userMetadata => userMetadata.UserMetadataId);
    }
}
