using Microsoft.Extensions.Logging;
using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Domain.Aggregates.User;
using Y.Authentication.Domain.DomainEvents.Base;
using Y.Authentication.Domain.Errors;
using Y.Authentication.Domain.Repositories;
using Y.Authentication.Domain.Services;
using Y.Authentication.Domain.Shared;
using Y.Contract.SharedKernel.Enums;

namespace Y.Authentication.Application.Users.UseCases.CreateUser;
internal sealed class CreateUserUseCaseHandler : IUseCaseHandler<CreateUserUseCase>
{
    private readonly ILogger<CreateUserUseCaseHandler> _logger;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventsDispatcher _domainEventsDispatcher;

    public CreateUserUseCaseHandler(
        ILogger<CreateUserUseCaseHandler> logger,
        IPasswordHasherService passwordHasherService,
        IRoleRepository roleRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IDomainEventsDispatcher domainEventsDispatcher)
    {
        _logger = logger;
        _passwordHasherService = passwordHasherService;
        _roleRepository = roleRepository;
        _userRepository = userRepository;
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

        var userRole = await _roleRepository.GetByNameAsync(Role.User.ToString(), cancellationToken);
        if (userRole is null)
        {
            return Result.Failure(UserErrors.UserRoleNotFound);
        }

        var (PasswordSalt, PasswordHash) = _passwordHasherService.HashPassword(request.Password);

        var userResult = User.Create(
            request.Email,
            PasswordHash,
            PasswordSalt,
            request.Name,
            request.BirthDate,
            userRole.Id);

        if (userResult.IsFailure)
        {
            return Result.Failure(userResult.Error);
        }

        await _userRepository.CreateAsync(userResult.Value, cancellationToken);
        await _userRepository.CreateMetadataAsync(userResult.Value.Metadata!, cancellationToken);
        await _userRepository.CreateRoleAsync(userResult.Value.Roles.First(), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _domainEventsDispatcher.DispatchAsync(userResult.Value.GetDomainEvents(), cancellationToken);

        _logger.LogInformation("User {UserId} successfully created", userResult.Value.Id);

        return Result.Success();
    }
}
