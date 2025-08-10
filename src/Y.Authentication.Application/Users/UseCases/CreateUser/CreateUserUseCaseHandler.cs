using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Domain.DomainEvents;
using Y.Authentication.Domain.DomainEvents.Base;
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
    private readonly IDomainEventsDispatcher _domainEventsDispatcher;

    public CreateUserUseCaseHandler(
        ILogger<CreateUserUseCaseHandler> logger,
        IUserRepository userRepository,
        IUserMetadataRepository userMetadataRepository,
        IUnitOfWork unitOfWork,
        IDomainEventsDispatcher domainEventsDispatcher)
    {
        _logger = logger;
        _userRepository = userRepository;
        _userMetadataRepository = userMetadataRepository;
        _unitOfWork = unitOfWork;
        _domainEventsDispatcher = domainEventsDispatcher;
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

        var createdUserId = await _userRepository.CreateAsync(user, cancellationToken);
        if (createdUserId == Guid.Empty)
        {
            return Result.Failure(UserErrors.UserCreationFailed);
        }

        var metadata = new UserMetadata
        {
            UserId = createdUserId,
            Name = request.Name,
            BirthDate = request.BirthDate,
        };

        var createdUserMetadataId = await _userMetadataRepository.CreateAsync(metadata, cancellationToken);
        if (createdUserMetadataId == Guid.Empty)
        {
            return Result.Failure(UserMetadataErrors.UserMetadataCreationFailed);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _domainEventsDispatcher.DispatchAsync(
            [new CreateUserRoleDomainEvent(createdUserId, Contract.Root.Authentication.Shared.Role.User),
            new SendUserEmailVerificationDomainEvent(createdUserId, user.Email, user.VerificationToken, metadata.Name ?? string.Empty)],
            cancellationToken);

        _logger.LogInformation("User {UserId} successfully created", createdUserId);

        return Result.Success();
    }

    private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }
}
