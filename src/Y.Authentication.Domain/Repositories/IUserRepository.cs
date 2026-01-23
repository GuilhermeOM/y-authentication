using Y.Authentication.Domain.Aggregates.User;

namespace Y.Authentication.Domain.Repositories;
public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetWithMetadataRolesByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetWithMetadataByVerificationTokenAsync(string verificationToken, CancellationToken cancellationToken = default);
    Task<User?> TrackByVerificationTokenAsync(string verificationToken, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(User user, CancellationToken cancellationToken = default);
    Task<Guid> CreateMetadataAsync(UserMetadata userMetadata, CancellationToken cancellationToken = default);
    Task<Guid> CreateRoleAsync(UserRole userRole, CancellationToken cancellationToken = default);
}
