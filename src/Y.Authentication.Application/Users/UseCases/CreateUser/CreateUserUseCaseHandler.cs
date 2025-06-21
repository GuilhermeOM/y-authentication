using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Domain.Shared;

namespace Y.Authentication.Application.Users.UseCases.CreateUser;
internal sealed class CreateUserUseCaseHandler : IUseCaseHandler<CreateUserUseCase>
{
    public async Task<Result> HandleAsync(CreateUserUseCase request, CancellationToken cancellationToken = default)
    {
        return Result.Failure(new Error("error", "error"));
    }
}
