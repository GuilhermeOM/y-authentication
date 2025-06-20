using Y.Authentication.Domain.Entities.Base;

namespace Y.Authentication.Domain.Entities;
public class UserMetadata : Entity
{
    public string Name { get; set; } = string.Empty;
    public required DateOnly BirthDate { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
