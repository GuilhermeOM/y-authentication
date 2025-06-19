using Y.Authentication.Domain.Shared;

namespace Y.Authentication.Application.Abstractions.Messaging;
public interface IUseCaseHandler<TRequest> where TRequest : IUseCase
{
    Task<Result> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

public interface IUseCaseHandler<TRequest, TResponse>
    where TRequest : IUseCase<TResponse>
    where TResponse : class
{
    Task<Result<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}
