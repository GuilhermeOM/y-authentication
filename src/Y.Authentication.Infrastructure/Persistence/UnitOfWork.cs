using Y.Authentication.Domain.Repositories;
using Y.Authentication.Domain.Shared;

namespace Y.Authentication.Infrastructure.Persistence;
internal sealed class UnitOfWork : IUnitOfWork
{
    public const string TransactionErrorCode = "TRANSACTION_ERROR";

    private readonly AppDataContext _context;

    public UnitOfWork(AppDataContext context)
    {
        _context = context;
    }

    public async Task<Result> TransactionAsync(
        Func<Task<Result>> action,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await action.Invoke();

            if (result.IsSuccess)
            {
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            else
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            return result;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure(new Error(TransactionErrorCode, "Some transaction error occurred"));
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
