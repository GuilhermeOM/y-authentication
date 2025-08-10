namespace Y.Authentication.Application.Users.DomainEvents.CreateUserRole.Exceptions;
public sealed class RoleNotCreatedException : Exception
{
    public RoleNotCreatedException(string message) : base(message)
    {
    }
}
