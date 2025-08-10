using Y.Authentication.Domain.DomainEvents.Base;

namespace Y.Authentication.Domain.DomainEvents;
public sealed record SendUserEmailVerificationDomainEvent(
    Guid UserId,
    string Email,
    string VerificationToken,
    string UserName = "") : IDomainEvent;

