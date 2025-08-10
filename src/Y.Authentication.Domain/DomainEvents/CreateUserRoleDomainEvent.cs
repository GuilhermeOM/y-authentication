using Y.Authentication.Domain.DomainEvents.Base;
using Y.Contract.Root.Authentication.Shared;

namespace Y.Authentication.Domain.DomainEvents;
public sealed record CreateUserRoleDomainEvent(Guid UserId, Role Role) : IDomainEvent;
