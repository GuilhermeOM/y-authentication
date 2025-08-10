using MassTransit;
using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Domain.DomainEvents;
using Y.Contract.Root.Core.Events;

namespace Y.Authentication.Application.Users.DomainEvents.CreateUserProfile;
internal sealed class CreateUserProfileDomainEventHandler : IDomainEventHandler<CreateUserProfileDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateUserProfileDomainEventHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task HandleAsync(CreateUserProfileDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _publishEndpoint.Publish(new CreateProfileEvent
        {
            CorrelationId = domainEvent.UserId.ToString(),
            UserId = domainEvent.UserId,
            Name = domainEvent.UserName ?? string.Empty
        }, cancellationToken);
    }
}
