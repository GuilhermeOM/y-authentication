using MassTransit;
using Moq;
using Y.Authentication.Application.Users.DomainEvents.CreateUserProfile;
using Y.Authentication.Domain.DomainEvents;
using Y.Contract.Root.Core.Events;

namespace Y.Authentication.UnitTest.Users.DomainEvents;
public class CreateUserProfileDomainEventHandlerTests
{
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;

    private readonly CreateUserProfileDomainEventHandler _handler;

    public CreateUserProfileDomainEventHandlerTests()
    {
        _publishEndpointMock = new Mock<IPublishEndpoint>();

        _handler = new CreateUserProfileDomainEventHandler(_publishEndpointMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishCreateProfileEvent()
    {
        // Arrange
        var domainEvent = new CreateUserProfileDomainEvent(Guid.NewGuid(), "dummyName");

        // Act
        await _handler.HandleAsync(domainEvent, default);

        // Assert
        _publishEndpointMock.Verify(
            mock => mock.Publish(
                It.Is<CreateProfileEvent>(@event =>
                    @event.CorrelationId == domainEvent.UserId.ToString() &&
                    @event.UserId == domainEvent.UserId &&
                    @event.Name == domainEvent.UserName),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
