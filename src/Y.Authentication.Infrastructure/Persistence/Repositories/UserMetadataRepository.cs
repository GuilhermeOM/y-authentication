using Y.Authentication.Domain.Entities;
using Y.Authentication.Domain.Repositories;

namespace Y.Authentication.Infrastructure.Persistence.Repositories;
internal sealed class UserMetadataRepository : IUserMetadataRepository
{
    private readonly AppDataContext _context;

    public UserMetadataRepository(AppDataContext context)
    {
        _context = context;
    }

    public async Task<Guid> CreateAsync(UserMetadata userMetadata, CancellationToken cancellationToken = default)
    {
        await _context.UsersMetadata.AddAsync(userMetadata, cancellationToken);
        return userMetadata.Id;
    }
}
