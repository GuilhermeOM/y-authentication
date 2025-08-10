using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Y.Authentication.Domain.Entities;

namespace Y.Authentication.Infrastructure.Persistence.Configurations;
internal sealed class UserMetadataConfiguration : IEntityTypeConfiguration<UserMetadata>
{
    public void Configure(EntityTypeBuilder<UserMetadata> builder)
    {
        builder.Property(prop => prop.BirthDate).IsRequired();

        builder.HasOne(entity => entity.User)
            .WithOne(entity => entity.Metadata)
            .HasForeignKey<UserMetadata>(userMetadata => userMetadata.UserId)
            .IsRequired();

        builder.Property(prop => prop.Name)
            .HasMaxLength(50);
    }
}
