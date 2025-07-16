using Y.Authentication.Domain.Entities;

namespace Y.Authentication.Domain.Repositories;
public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetWithMetadataByVerificationTokenAsync(string verificationToken, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> VerifyAsync(Guid id, CancellationToken cancellationToken = default);
}
