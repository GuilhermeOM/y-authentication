using Y.Authentication.Domain.Entities;

namespace Y.Authentication.Domain.Repositories;
public interface IUserRoleRepository
{
    Task<bool> ExistsAsync(Guid userId, Guid roleId, CancellationToken cancellation = default);
    Task<Guid> CreateAsync(UserRole userRole, CancellationToken cancellationToken = default);
}
