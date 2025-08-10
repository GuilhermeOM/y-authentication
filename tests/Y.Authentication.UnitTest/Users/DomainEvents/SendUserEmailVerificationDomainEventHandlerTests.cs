using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Y.Authentication.Application.Users.DomainEvents.SendUserEmailVerification;
using Y.Authentication.Domain.Constants;
using Y.Authentication.Domain.DomainEvents;
using Y.Contract.Root.Notification.Events;
using Y.Contract.Root.Notification.Shared;

namespace Y.Authentication.UnitTest.Users.DomainEvents;
public class SendUserEmailVerificationDomainEventHandlerTests
{
    private readonly Mock<ILogger<SendUserEmailVerificationDomainEventHandler>> _loggerMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;

    private readonly SendUserEmailVerificationDomainEventHandler _handler;

    public SendUserEmailVerificationDomainEventHandlerTests()
    {
        _loggerMock = new Mock<ILogger<SendUserEmailVerificationDomainEventHandler>>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();

        _httpContextAccessorMock
            .Setup(mock => mock.HttpContext)
            .Returns(new DefaultHttpContext());

        _handler = new SendUserEmailVerificationDomainEventHandler(
            _loggerMock.Object,
            _httpContextAccessorMock.Object,
            _publishEndpointMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishSendEmailEvent()
    {
        // Arrange
        var domainEvent = new SendUserEmailVerificationDomainEvent(Guid.NewGuid(), "dummy@dummy.com", "dummyToken", "dummyName");

        // Act
        await _handler.HandleAsync(domainEvent, default);

        // Assert
        _publishEndpointMock
            .Verify(mock => mock.Publish(
                It.Is<SendEmailEvent>(@event =>
                    @event.Email == domainEvent.Email
                    && @event.Template == EmailTemplate.AccountVerification
                    && ContainsRequiredProperties(@event.Properties, domainEvent)),
                It.IsAny<IPipe<PublishContext<SendEmailEvent>>>(),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    private static bool ContainsRequiredProperties(Dictionary<string, string> properties, SendUserEmailVerificationDomainEvent domainEvent)
    {
        return properties.ContainsKey("VerificationLink")
            && properties["VerificationLink"].Contains($"api/user/{UserConstants.VerifyEndpoint}?VerificationToken")
            && properties.ContainsKey("UserName")
            && properties["UserName"] == (domainEvent.UserName ?? string.Empty);
    }
}
