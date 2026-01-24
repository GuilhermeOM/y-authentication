using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Repositories;
using Y.Authentication.Domain.Shared;

namespace Y.Authentication.Application.Users.UseCases.VerifyUser;
internal sealed class VerifyUserUseCaseHandler : IUseCaseHandler<VerifyUserUseCase>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyUserUseCaseHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(VerifyUserUseCase request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.TrackByVerificationTokenAsync(request.VerificationToken, cancellationToken);
        if (user is null)
        {
            return Result.Failure(UserErrors.UserNotFound);
        }

        var verifyResult = user.Verify();
        if (verifyResult.IsFailure)
        {
            return Result.Failure(verifyResult.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
