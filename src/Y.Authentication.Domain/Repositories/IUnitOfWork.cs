using Y.Authentication.Domain.Shared;

namespace Y.Authentication.Domain.Repositories;
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
