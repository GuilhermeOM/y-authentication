using System.Security.Cryptography;
using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Domain.Constants;
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
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateUserUseCaseHandler(
        ILogger<CreateUserUseCaseHandler> logger,
        IPublishEndpoint publishEndpoint,
        IUserRepository userRepository,
        IUserMetadataRepository userMetadataRepository,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _userRepository = userRepository;
        _userMetadataRepository = userMetadataRepository;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
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

            await SendAccountVerificationEmailAsync(user, request.Name, cancellationToken);

            return Result.Success();
        }, cancellationToken);
    }

    private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    private async Task SendAccountVerificationEmailAsync(
        User user,
        string? userName,
        CancellationToken cancellationToken = default)
    {
        var @event = new SendEmailEvent
        {
            CorrelationId = user.Id.ToString(),
            Email = user.Email,
            Template = EmailTemplate.ACCOUNT_VERIFICATION,
            Properties = new Dictionary<string, string>
            {
                { "VerificationLink", CreateAccountVerificationLink(user.VerificationToken) },
                { "UserName", userName ?? string.Empty }
            }
        };

        await _publishEndpoint.Publish(@event, callback =>
        {
            callback.SetRoutingKey(SendEmailEvent.RoutingKey);
        }, cancellationToken);
    }

    private string CreateAccountVerificationLink(string verificationToken)
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request is null)
        {
            _logger.LogError("HttpContext or Request is null, cannot create verification link");
            return string.Empty;
        }

        return $"{request.Scheme}://{request.Host}{request.PathBase}/api/user/{UserConstants.VerifyEndpoint}?VerificationToken={verificationToken}";
    }
}
