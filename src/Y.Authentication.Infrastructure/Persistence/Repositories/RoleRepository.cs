using Microsoft.EntityFrameworkCore;
using Y.Authentication.Domain.Aggregates.Role;
using Y.Authentication.Domain.Repositories;

namespace Y.Authentication.Infrastructure.Persistence.Repositories;
internal sealed class RoleRepository : IRoleRepository
{
    private readonly AppDataContext _context;

    public RoleRepository(AppDataContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Roles.AsNoTracking().FirstOrDefaultAsync(role => role.Name == name, cancellationToken);
    }

    public async Task<Guid> CreateAsync(Role role, CancellationToken cancellationToken = default)
    {
        await _context.Roles.AddAsync(role, cancellationToken);
        return role.Id;
    }

    public async Task CreateManyAsync(IEnumerable<Role> roles, CancellationToken cancellationToken = default)
    {
        await _context.Roles.AddRangeAsync(roles, cancellationToken);
    }
}
