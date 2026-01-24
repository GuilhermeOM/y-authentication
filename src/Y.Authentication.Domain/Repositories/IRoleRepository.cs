using Y.Authentication.Domain.Aggregates.Role;

namespace Y.Authentication.Domain.Repositories;
public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(Role role, CancellationToken cancellationToken = default);
    Task CreateManyAsync(IEnumerable<Role> roles, CancellationToken cancellationToken = default);
}
