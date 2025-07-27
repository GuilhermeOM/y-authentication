using Y.Authentication.Domain.Entities.Base;

namespace Y.Authentication.Domain.Entities;
public class UserRole : Entity
{
    public required Guid UserId { get; set; }
    public required Guid RoleId { get; set; }
}
