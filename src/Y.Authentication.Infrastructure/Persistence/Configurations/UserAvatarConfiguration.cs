using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Y.Authentication.Domain.Aggregates.User;

namespace Y.Authentication.Infrastructure.Persistence.Configurations;

internal sealed class UserAvatarConfiguration : IEntityTypeConfiguration<UserAvatar>
{
    public void Configure(EntityTypeBuilder<UserAvatar> builder)
    {
        builder.HasOne(entity => entity.User)
            .WithOne(entity => entity.Avatar)
            .HasForeignKey<UserAvatar>(userAvatar => userAvatar.UserId)
            .IsRequired();

        builder.Property(prop => prop.Mime).IsRequired();
        builder.Property(prop => prop.Description).HasMaxLength(250);
    }
}
