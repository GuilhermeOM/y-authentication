using Microsoft.EntityFrameworkCore;
using Y.Authentication.Domain.Entities;
using Y.Authentication.Domain.Repositories;

namespace Y.Authentication.Infrastructure.Persistence.Repositories;
internal sealed class UserRoleRepository : IUserRoleRepository
{
    private readonly AppDataContext _context;

    public UserRoleRepository(AppDataContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid roleId, CancellationToken cancellation = default)
    {
        return await _context.UsersRoles
            .AsNoTracking()
            .AnyAsync(userRole => userRole.UserId == userId && userRole.RoleId == roleId, cancellation);
    }

    public async Task<Guid> CreateAsync(UserRole userRole, CancellationToken cancellationToken = default)
    {
        await _context.UsersRoles.AddAsync(userRole, cancellationToken);
        return userRole.Id;
    }
}
