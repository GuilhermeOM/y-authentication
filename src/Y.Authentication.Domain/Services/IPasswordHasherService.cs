using Y.Authentication.Domain.ValueObjects;

namespace Y.Authentication.Domain.Services;

public interface IPasswordHasherService
{
    PasswordHash HashPassword(string password);
    bool IsPasswordSequenceEqual(string password, byte[] salt, byte[] hash);
}
