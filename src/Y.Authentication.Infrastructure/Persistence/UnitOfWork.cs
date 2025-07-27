using Y.Authentication.Domain.Repositories;

namespace Y.Authentication.Infrastructure.Persistence;
internal sealed class UnitOfWork : IUnitOfWork
{
    public const string TransactionErrorCode = "TRANSACTION_ERROR";

    private readonly AppDataContext _context;

    public UnitOfWork(AppDataContext context)
    {
        _context = context;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
