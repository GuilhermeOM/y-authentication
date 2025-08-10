using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Domain.DomainEvents;
using Y.Authentication.Domain.DomainEvents.Base;
using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Repositories;
using Y.Authentication.Domain.Shared;

namespace Y.Authentication.Application.Users.UseCases.VerifyUser;
internal sealed class VerifyUserUseCaseHandler : IUseCaseHandler<VerifyUserUseCase>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventsDispatcher _domainEventsDispatcher;

    public VerifyUserUseCaseHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IDomainEventsDispatcher domainEventsDispatcher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _domainEventsDispatcher = domainEventsDispatcher;
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

        var didVerify = await _userRepository.VerifyAsync(user.Id, cancellationToken);
        if (!didVerify)
        {
            return Result.Failure(UserErrors.UserVerificationFailed);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _domainEventsDispatcher.DispatchAsync(
            [new CreateUserProfileDomainEvent(user.Id, user.Metadata?.Name ?? string.Empty)],
            cancellationToken);

        return Result.Success();
    }
}
