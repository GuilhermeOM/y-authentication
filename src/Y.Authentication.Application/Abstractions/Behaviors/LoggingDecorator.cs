using Microsoft.Extensions.Logging;
using Serilog.Context;
using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Domain.Shared;

namespace Y.Authentication.Application.Abstractions.Behaviors;
internal static class LoggingDecorator
{
    internal sealed class UseCaseHandler<TRequest> : IUseCaseHandler<TRequest>
        where TRequest : IUseCase
    {
        private readonly IUseCaseHandler<TRequest> _innerHandler;
        private readonly ILogger<UseCaseHandler<TRequest>> _logger;

        public UseCaseHandler(
            IUseCaseHandler<TRequest> innerHandler,
            ILogger<UseCaseHandler<TRequest>> logger)
        {
            _innerHandler = innerHandler;
            _logger = logger;
        }

        public async Task<Result> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var useCaseName = typeof(TRequest).Name;

            using var _ = LogContext.PushProperty("UseCaseName", useCaseName);

            _logger.LogInformation("Processing use case {UseCaseName}", useCaseName);

            var result = await _innerHandler.HandleAsync(request, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Completed use case {UseCaseName}", useCaseName);
            }
            else
            {
                using (LogContext.PushProperty("Error", result.Error, true))
                {
                    _logger.LogError("Completed use case {UseCaseName} with error", useCaseName);
                }
            }
            return result;
        }
    }

    internal sealed class UseCaseHandler<TRequest, TResponse>: IUseCaseHandler<TRequest, TResponse>
        where TRequest : IUseCase<TResponse>
        where TResponse : class
    {
        private readonly IUseCaseHandler<TRequest, TResponse> _innerHandler;
        private readonly ILogger<UseCaseHandler<TRequest, TResponse>> _logger;

        public UseCaseHandler(
            IUseCaseHandler<TRequest, TResponse> innerHandler,
            ILogger<UseCaseHandler<TRequest, TResponse>> logger)
        {
            _innerHandler = innerHandler;
            _logger = logger;
        }

        public async Task<Result<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var useCaseName = typeof(TRequest).Name;

            using var _ = LogContext.PushProperty("UseCaseName", useCaseName);

            _logger.LogInformation("Processing use case {UseCaseName}", useCaseName);

            var result = await _innerHandler.HandleAsync(request, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Completed use case {UseCaseName}", useCaseName);
            }
            else
            {
                using (LogContext.PushProperty("Error", result.Error, true))
                {
                    _logger.LogError("Completed use case {UseCaseName} with error", useCaseName);
                }
            }
            return result;
        }
    }
}
