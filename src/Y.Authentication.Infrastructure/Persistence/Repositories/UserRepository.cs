using Microsoft.EntityFrameworkCore;
using Y.Authentication.Domain.Aggregates.User;
using Y.Authentication.Domain.Repositories;

namespace Y.Authentication.Infrastructure.Persistence.Repositories;
internal sealed class UserRepository : IUserRepository
{
    private readonly AppDataContext _context;

    public UserRepository(AppDataContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AsNoTracking().AnyAsync(user => user.Email == email, cancellationToken);
    }

    public async Task<User?> GetWithMetadataRolesByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .Include(user => user.Metadata)
            .Include(user => user.Roles)
            .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    public async Task<User?> GetWithMetadataByVerificationTokenAsync(string verificationToken, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .Include(user => user.Metadata)
            .SingleOrDefaultAsync(user => user.VerificationToken == verificationToken, cancellationToken);
    }

    public async Task<User> TrackByVerificationTokenAsync(string verificationToken, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .SingleAsync(user => user.VerificationToken == verificationToken, cancellationToken);
    }

    public async Task<Guid> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        return user.Id;
    }

    public async Task<Guid> CreateMetadataAsync(UserMetadata userMetadata, CancellationToken cancellationToken = default)
    {
        await _context.UsersMetadata.AddAsync(userMetadata, cancellationToken);
        return userMetadata.Id;
    }

    public async Task<Guid> CreateRoleAsync(UserRole userRole, CancellationToken cancellationToken = default)
    {
        await _context.UsersRoles.AddAsync(userRole, cancellationToken);
        return userRole.Id;
    }
}
