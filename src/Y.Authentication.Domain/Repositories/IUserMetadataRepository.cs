using Y.Authentication.Domain.Entities;

namespace Y.Authentication.Domain.Repositories;
public interface IUserMetadataRepository
{
    Task<Guid> CreateAsync(UserMetadata userMetadata, CancellationToken cancellationToken = default);
}
