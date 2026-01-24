using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Y.Authentication.Application.Users.DomainEvents;
using Y.Authentication.Domain.Constants;
using Y.Authentication.Domain.DomainEvents;
using Y.Authentication.Domain.Services;
using Y.Contract.SharedKernel.Abstractions.Messaging;
using Y.Contract.SharedKernel.Events;

namespace Y.Authentication.UnitTest.Users.DomainEvents;
public class SendUserEmailVerificationDomainEventHandlerTests
{
    private readonly Mock<ILogger<SendUserEmailVerificationDomainEventHandler>> _loggerMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IProducerService> _producerService;

    private readonly SendUserEmailVerificationDomainEventHandler _handler;

    public SendUserEmailVerificationDomainEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<SendUserEmailVerificationDomainEventHandler>>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _producerService = new Mock<IProducerService>();

        _httpContextAccessorMock
            .Setup(mock => mock.HttpContext)
            .Returns(new DefaultHttpContext());

        _handler = new SendUserEmailVerificationDomainEventHandler(
            _loggerMock.Object,
            _httpContextAccessorMock.Object,
            _producerService.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishSendEmailEvent()
    {
        // Arrange
        var domainEvent = new SendUserEmailVerificationDomainEvent(Guid.NewGuid(), "dummy@dummy.com", "dummyToken", "dummyName");

        // Act
        await _handler.HandleAsync(domainEvent, default);

        // Assert
        _producerService
            .Verify(mock => mock.ProduceAsync(
                It.Is<NotifyChannelEvent>(@event =>
                    @event.Channel == Channel.Email
                    && @event.EmailTemplate == EmailTemplate.AccountVerification
                    && ContainsRequiredProperties(@event.Properties, domainEvent)),
                It.Is<MessageMetadata>(metadata =>
                    metadata.MessageKey == domainEvent.UserId.ToString()
                    && metadata.Topic == KafkaConstants.Topics.NotifyChannelTopic)), Times.Once);
    }

    private static bool ContainsRequiredProperties(Dictionary<string, string> properties, SendUserEmailVerificationDomainEvent domainEvent)
    {
        return properties.ContainsKey("Email")
            && properties["Email"] == domainEvent.Email
            && properties.ContainsKey("VerificationLink")
            && properties["VerificationLink"].Contains($"api/user/{UserConstants.VerifyEndpoint}?VerificationToken")
            && properties.ContainsKey("UserName")
            && properties["UserName"] == (domainEvent.UserName ?? string.Empty);
    }
}
