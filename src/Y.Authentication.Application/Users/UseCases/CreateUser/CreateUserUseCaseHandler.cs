using Microsoft.Extensions.Logging;
using Serilog.Context;
using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Application.Users.Services.CreateUserAvatar;
using Y.Authentication.Domain.Aggregates.User;
using Y.Authentication.Domain.DomainEvents.Base;
using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Repositories;
using Y.Authentication.Domain.Services;
using Y.Authentication.Domain.Shared;
using Y.Authentication.Domain.ValueObjects;
using Y.Contract.SharedKernel.Enums;

namespace Y.Authentication.Application.Users.UseCases.CreateUser;
internal sealed class CreateUserUseCaseHandler : IUseCaseHandler<CreateUserUseCase>
{
    private FileUploadResult? _uploadedAvatar = null;

    private readonly ILogger<CreateUserUseCaseHandler> _logger;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly ICreateUserAvatarService _createUserAvatarService;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventsDispatcher _domainEventsDispatcher;

    public CreateUserUseCaseHandler(
        ILogger<CreateUserUseCaseHandler> logger,
        IPasswordHasherService passwordHasherService,
        ICreateUserAvatarService createUserAvatarService,
        IRoleRepository roleRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IDomainEventsDispatcher domainEventsDispatcher)
    {
        _logger = logger;
        _passwordHasherService = passwordHasherService;
        _createUserAvatarService = createUserAvatarService;
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _domainEventsDispatcher = domainEventsDispatcher;
    }

    public async Task<Result> HandleAsync(CreateUserUseCase request, CancellationToken cancellationToken = default)
    {
        using var _ = LogContext.PushProperty(nameof(request.Email), request.Email);
        
        var userExists = await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken);
        if (userExists)
        {
            return Result.Failure(UserErrors.UserAlreadyExists);
        }

        var userRole = await _roleRepository.GetByNameAsync(Role.User.ToString(), cancellationToken);
        if (userRole is null)
        {
            return Result.Failure(UserErrors.UserRoleNotFound);
        }

        var passwordHash = _passwordHasherService.HashPassword(request.Password);
        try
        {
            var avatarUploadResult = await _createUserAvatarService.UploadAsync(request.AvatarPhoto, cancellationToken);
            if (avatarUploadResult.IsFailure)
            {
                return Result.Failure(avatarUploadResult.Error);
            }

            _uploadedAvatar = avatarUploadResult.Value;

            var userResult = User.Create(
                passwordHash,
                request.Email,
                request.BirthDate,
                userRole.Id,
                request.Name,
                avatarUploadResult.Value);

            if (userResult.IsFailure)
            {
                return Result.Failure(userResult.Error);
            }

            await _userRepository.CreateAsync(userResult.Value, cancellationToken);
            await _userRepository.CreateMetadataAsync(userResult.Value.Metadata!, cancellationToken);
            await _userRepository.CreateRoleAsync(userResult.Value.Roles.First(), cancellationToken);

            if (avatarUploadResult.Value is not null)
            {
                await _userRepository.CreateAvatarAsync(userResult.Value.Avatar!, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _domainEventsDispatcher.DispatchAsync(userResult.Value.GetDomainEvents(), cancellationToken);

            _logger.LogInformation("User {UserId} successfully created", userResult.Value.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating the user");

            await _createUserAvatarService.RollbackUploadAsync(_uploadedAvatar);
            return Result.Failure(UserErrors.UserCreationFailed);
        }
    }
}
