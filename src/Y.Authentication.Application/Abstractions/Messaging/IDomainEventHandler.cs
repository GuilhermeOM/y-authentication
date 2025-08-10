using Y.Authentication.Domain.DomainEvents.Base;

namespace Y.Authentication.Application.Abstractions.Messaging;
public interface IDomainEventHandler<T> where T : IDomainEvent
{
    Task HandleAsync(T domainEvent, CancellationToken cancellationToken = default);
}
