using Microsoft.Extensions.Logging;
using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Domain.Shared;

namespace Y.Authentication.Application.Users.UseCases.Example;
internal class ExampleUseCaseHandler : IUseCaseHandler<ExampleUseCase, ExampleUseCaseResponse>
{
    private readonly ILogger<ExampleUseCaseHandler> _logger;

    public ExampleUseCaseHandler(ILogger<ExampleUseCaseHandler> logger)
    {
        _logger = logger;
    }

    public async Task<Result<ExampleUseCaseResponse>> HandleAsync(ExampleUseCase request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling ExampleUseCase with request: {@Request}", request);

        await Task.Delay(1000, cancellationToken);
        return Result.Success(new ExampleUseCaseResponse($"Hello {request.ExampleRequest}"));
    }
}
