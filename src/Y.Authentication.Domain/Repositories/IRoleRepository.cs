using Y.Authentication.Domain.Entities;

namespace Y.Authentication.Domain.Repositories;
public interface IRoleRepository
{
    Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(Role role, CancellationToken cancellationToken = default);
}
