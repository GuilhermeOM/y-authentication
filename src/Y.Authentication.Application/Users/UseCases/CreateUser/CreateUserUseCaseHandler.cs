using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Domain.Entities;
using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Repositories;
using Y.Authentication.Domain.Shared;

namespace Y.Authentication.Application.Users.UseCases.CreateUser;
internal sealed class CreateUserUseCaseHandler : IUseCaseHandler<CreateUserUseCase>
{
    private readonly ILogger<CreateUserUseCaseHandler> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IUserMetadataRepository _userMetadataRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserUseCaseHandler(
        ILogger<CreateUserUseCaseHandler> logger,
        IUserRepository userRepository,
        IUserMetadataRepository userMetadataRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
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

            _logger.LogInformation("User {UserId} created successfully", createdUserId);
            return Result.Success();
        }, cancellationToken);
    }

    private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }
}
