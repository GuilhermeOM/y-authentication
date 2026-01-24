using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Domain.Constants;
using Y.Authentication.Domain.DomainEvents;
using Y.Authentication.Domain.Services;
using Y.Contract.SharedKernel.Abstractions.Messaging;
using Y.Contract.SharedKernel.Events;

namespace Y.Authentication.Application.Users.DomainEvents;
internal sealed class SendUserEmailVerificationDomainEventHandler : IDomainEventHandler<SendUserEmailVerificationDomainEvent>
{
    private readonly ILogger<SendUserEmailVerificationDomainEventHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IProducerService _producerService;

    public SendUserEmailVerificationDomainEventHandler(
        ILogger<SendUserEmailVerificationDomainEventHandler>  logger,
        IHttpContextAccessor httpContextAccessor,
        IProducerService producerService)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _producerService = producerService;
    }

    public async Task HandleAsync(SendUserEmailVerificationDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var @event = new NotifyChannelEvent
        {
            Channel = Channel.Email,
            EmailTemplate = EmailTemplate.AccountVerification,
            Properties = new Dictionary<string, string>
            {
                { "Email", domainEvent.Email },
                { "VerificationLink", CreateAccountVerificationLink(domainEvent.VerificationToken) },
                { "UserName", domainEvent.UserName ?? string.Empty }
            }
        };

        await _producerService.ProduceAsync(@event, new MessageMetadata
        {
            MessageKey = domainEvent.UserId.ToString(),
            Topic = KafkaConstants.Topics.NotifyChannelTopic,
        });
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
