namespace Y.Authentication.Domain.Services;

public interface IPasswordHasherService
{
    (byte[] Salt, byte[] Hash) HashPassword(string password);
    bool IsPasswordSequenceEqual(string password, byte[] salt, byte[] hash);
}
