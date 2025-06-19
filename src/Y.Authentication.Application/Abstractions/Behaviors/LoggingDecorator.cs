using Microsoft.Extensions.Logging;
using Serilog.Context;
using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Domain.Shared;

namespace Y.Authentication.Application.Abstractions.Behaviors;
internal static class LoggingDecorator
{
    internal sealed class UseCaseHandler<TRequest>(
        IUseCaseHandler<TRequest> innerHandler,
        ILogger<UseCaseHandler<TRequest>> logger) : IUseCaseHandler<TRequest> where TRequest : IUseCase
    {
        public async Task<Result> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var useCaseName = typeof(TRequest).Name;

            using var _ = LogContext.PushProperty("UseCaseName", useCaseName);

            logger.LogInformation("Processing use case {UseCaseName}", useCaseName);

            var result = await innerHandler.HandleAsync(request, cancellationToken);

            if (result.IsSuccess)
            {
                logger.LogInformation("Completed use case {UseCaseName}", useCaseName);
            }
            else
            {
                using (LogContext.PushProperty("Error", result.Error, true))
                {
                    logger.LogError("Completed use case {UseCaseName} with error", useCaseName);
                }
            }
            return result;
        }
    }

    internal sealed class UseCaseHandler<TRequest, TResponse>(
        IUseCaseHandler<TRequest, TResponse> innerHandler,
        ILogger<UseCaseHandler<TRequest, TResponse>> logger) : IUseCaseHandler<TRequest, TResponse>
        where TRequest : IUseCase<TResponse>
        where TResponse : class
    {
        public async Task<Result<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var useCaseName = typeof(TRequest).Name;

            using var _ = LogContext.PushProperty("UseCaseName", useCaseName);

            logger.LogInformation("Processing use case {UseCaseName}", useCaseName);

            var result = await innerHandler.HandleAsync(request, cancellationToken);

            if (result.IsSuccess)
            {
                logger.LogInformation("Completed use case {UseCaseName}", useCaseName);
            }
            else
            {
                using (LogContext.PushProperty("Error", result.Error, true))
                {
                    logger.LogError("Completed use case {UseCaseName} with error", useCaseName);
                }
            }
            return result;
        }
    }
}
