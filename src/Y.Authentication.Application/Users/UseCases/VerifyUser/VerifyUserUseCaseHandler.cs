using MassTransit;
using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Repositories;
using Y.Authentication.Domain.Shared;
using Y.Contract.Root.Core.Events;

namespace Y.Authentication.Application.Users.UseCases.VerifyUser;
internal sealed class VerifyUserUseCaseHandler : IUseCaseHandler<VerifyUserUseCase>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyUserUseCaseHandler(
        IPublishEndpoint publishEndpoint,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _publishEndpoint = publishEndpoint;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(VerifyUserUseCase request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetWithMetadataByVerificationTokenAsync(request.VerificationToken, cancellationToken);
        if (user is null)
        {
            return Result.Failure(UserErrors.UserNotFound);
        }

        if (user.VerifiedAt is not null)
        {
            return Result.Failure(UserErrors.UserAlreadyVerified);
        }

        return await _unitOfWork.TransactionAsync(async () =>
        {
            var didVerify = await _userRepository.VerifyAsync(user.Id, cancellationToken);
            if (!didVerify)
            {
                return Result.Failure(UserErrors.UserVerificationFailed);
            }
            await SendCreateProfileEventAsync(user.Id, user.Metadata?.Name, cancellationToken);

            return Result.Success();
        }, cancellationToken);
    }

    private async Task SendCreateProfileEventAsync(Guid userId, string? name, CancellationToken cancellationToken = default)
    {
        await _publishEndpoint.Publish(new CreateProfileEvent
        {
            CorrelationId = userId.ToString(),
            UserId = userId,
            Name = name ?? string.Empty
        }, cancellationToken);
    }
}
