using System.Security.Cryptography;
using System.Text;
using MassTransit;
using Microsoft.Extensions.Logging;
using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Domain.Entities;
using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Repositories;
using Y.Authentication.Domain.Shared;
using Y.Contract.Root.Notification.Events;
using Y.Contract.Root.Notification.Shared;

namespace Y.Authentication.Application.Users.UseCases.CreateUser;
internal sealed class CreateUserUseCaseHandler : IUseCaseHandler<CreateUserUseCase>
{
    private readonly ILogger<CreateUserUseCaseHandler> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IUserRepository _userRepository;
    private readonly IUserMetadataRepository _userMetadataRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserUseCaseHandler(
        ILogger<CreateUserUseCaseHandler> logger,
        IPublishEndpoint publishEndpoint,
        IUserRepository userRepository,
        IUserMetadataRepository userMetadataRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _userRepository = userRepository;
        _userMetadataRepository = userMetadataRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(CreateUserUseCase request, CancellationToken cancellationToken = default)
    {
        var userExists = await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken);
        if (userExists)
        {
            return Result.Failure(UserErrors.UserAlreadyExists);
        }

        CreatePasswordHash(request.Password, out var passwordHash, out var passwordSalt);

        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
        };

        var metadata = new UserMetadata
        {
            Name = request.Name,
            BirthDate = request.BirthDate,
        };

        return await _unitOfWork.TransactionAsync(async () =>
        {
            var createdUserId = await _userRepository.CreateAsync(user, cancellationToken);
            if (createdUserId == Guid.Empty)
            {
                _logger.LogError("Failed to create user");
                return Result.Failure(UserErrors.UserCreationFailed);
            }

            metadata.UserId = createdUserId;

            var createdUserMetadataId = await _userMetadataRepository.CreateAsync(metadata, cancellationToken);
            if (createdUserMetadataId == Guid.Empty)
            {
                _logger.LogError("Failed to create metadata for user {UserId}", createdUserId);
                return Result.Failure(UserMetadataErrors.UserMetadataCreationFailed);
            }

            await SendActivationEmailAsync(request.Email, createdUserId, cancellationToken);

            return Result.Success();
        }, cancellationToken);
    }

    private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    private async Task SendActivationEmailAsync(string email, Guid userId, CancellationToken cancellationToken = default)
    {
        await _publishEndpoint.Publish(new SendEmailEvent
        {
            CorrelationId = userId.ToString(),
            Email = email,
            Template = EmailTemplate.ACCOUNT_ACTIVATION
        }, callback =>
        {
            callback.SetRoutingKey(SendEmailEvent.RoutingKey);
        }, cancellationToken);
    }
}
