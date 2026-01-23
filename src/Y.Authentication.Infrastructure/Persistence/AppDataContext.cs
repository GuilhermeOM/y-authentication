using Microsoft.EntityFrameworkCore;
using Y.Authentication.Domain.Aggregates.Role;
using Y.Authentication.Domain.Aggregates.User;

namespace Y.Authentication.Infrastructure.Persistence;
internal class AppDataContext : DbContext
{
    public AppDataContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserMetadata> UsersMetadata { get; set; }
    public DbSet<UserRole> UsersRoles { get; set; }
    public DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDataContext).Assembly);
    }
}
