using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Domain.Constants;
using Y.Authentication.Domain.DomainEvents;
using Y.Contract.Root.Notification.Events;
using Y.Contract.Root.Notification.Shared;

namespace Y.Authentication.Application.Users.DomainEvents.SendUserEmailVerification;
internal sealed class SendUserEmailVerificationDomainEventHandler : IDomainEventHandler<SendUserEmailVerificationDomainEvent>
{
    private readonly ILogger<SendUserEmailVerificationDomainEventHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPublishEndpoint _publishEndpoint;

    public SendUserEmailVerificationDomainEventHandler(
        ILogger<SendUserEmailVerificationDomainEventHandler>  logger,
        IHttpContextAccessor httpContextAccessor,
        IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _publishEndpoint = publishEndpoint;
    }

    public async Task HandleAsync(SendUserEmailVerificationDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var @event = new SendEmailEvent
        {
            CorrelationId = domainEvent.UserId.ToString(),
            Email = domainEvent.Email,
            Template = EmailTemplate.AccountVerification,
            Properties = new Dictionary<string, string>
            {
                { "VerificationLink", CreateAccountVerificationLink(domainEvent.VerificationToken) },
                { "UserName", domainEvent.UserName ?? string.Empty }
            }
        };

        await _publishEndpoint.Publish(@event, callback =>
        {
            callback.SetRoutingKey(SendEmailEvent.RoutingKey);
        }, cancellationToken);
    }

    private string CreateAccountVerificationLink(string verificationToken)
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request is null)
        {
            _logger.LogError("HttpContext or Request is null, cannot create verification link");
            return string.Empty;
        }

        return $"{request.Scheme}://{request.Host}{request.PathBase}/api/user/{UserConstants.VerifyEndpoint}?VerificationToken={verificationToken}";
    }
}
