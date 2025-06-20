using Microsoft.EntityFrameworkCore;
using Y.Authentication.Domain.Entities;

namespace Y.Authentication.Infrastructure.Persistence;
internal sealed class AppDataContext : DbContext
{
    public AppDataContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserMetadata> UsersMetadata { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDataContext).Assembly);
    }
}
