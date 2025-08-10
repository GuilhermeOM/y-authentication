using Y.Authentication.Domain.DomainEvents.Base;

namespace Y.Authentication.Domain.DomainEvents;
public sealed record CreateUserProfileDomainEvent(Guid UserId, string UserName = "") : IDomainEvent;
