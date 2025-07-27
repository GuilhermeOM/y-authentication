using Y.Authentication.Domain.Entities.Base;

namespace Y.Authentication.Domain.Entities;
public class Role : Entity
{
    public required string Name { get; set; }
}
