namespace Y.Authentication.Application.Users.DomainEvents.CreateUserRole.Exceptions;
public sealed class UserRoleNotCreatedException : Exception
{
    public UserRoleNotCreatedException(string message) : base(message)
    {
    }
}
