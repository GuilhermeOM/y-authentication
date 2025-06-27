using Y.Authentication.Domain.Shared;

namespace Y.Authentication.Domain.Repositories;
public interface IUnitOfWork
{
    Task<Result> TransactionAsync(Func<Task<Result>> action, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
