using Microsoft.EntityFrameworkCore;
using Y.Authentication.Domain.Entities;
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
            .Where(user => user.Email == email)
            .Include(user => user.Metadata)
            .Include(user => user.Roles)
            .ThenInclude(userRole => userRole.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetWithMetadataByVerificationTokenAsync(string verificationToken, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(user => user.VerificationToken == verificationToken)
            .Include(user => user.Metadata)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        return user.Id;
    }

    public async Task<bool> VerifyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Where(user => user.Id == id && user.VerifiedAt == null)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return false;
        }

        var currentUtcTime = DateTime.UtcNow;
        user.VerifiedAt = currentUtcTime;
        user.UpdatedAt = currentUtcTime;
        return true;
    }
}
