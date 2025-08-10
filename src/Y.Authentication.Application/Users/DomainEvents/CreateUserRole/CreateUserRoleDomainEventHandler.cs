using Microsoft.Extensions.Logging;
using Y.Authentication.Application.Abstractions.Messaging;
using Y.Authentication.Application.Users.DomainEvents.CreateUserRole.Exceptions;
using Y.Authentication.Domain.DomainEvents;
using Y.Authentication.Domain.Entities;
using Y.Authentication.Domain.Repositories;

namespace Y.Authentication.Application.Users.DomainEvents.CreateUserRole;
internal sealed class CreateUserRoleDomainEventHandler : IDomainEventHandler<CreateUserRoleDomainEvent>
{
    private readonly ILogger<CreateUserRoleDomainEventHandler> _logger;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserRoleDomainEventHandler(
        ILogger<CreateUserRoleDomainEventHandler> logger,
        IUserRoleRepository userRoleRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _userRoleRepository = userRoleRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(CreateUserRoleDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var roleId = await GerRoleIdAsync(domainEvent.Role.ToString(), cancellationToken);

        var existsUserWithRole = await _userRoleRepository.ExistsAsync(domainEvent.UserId, roleId, cancellationToken);
        if (existsUserWithRole)
        {
            _logger.LogInformation("User {UserId} already has role {RoleName}", domainEvent.UserId, domainEvent.Role.ToString());
            return;
        }

        var userRole = new UserRole
        {
            UserId = domainEvent.UserId,
            RoleId = roleId
        };

        var createdUserRoleId = await _userRoleRepository.CreateAsync(userRole, cancellationToken);
        if (createdUserRoleId == Guid.Empty)
        {
            throw new UserRoleNotCreatedException($"Failed to create user role for UserId: {domainEvent.UserId}, Role: {domainEvent.Role}");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Guid> GerRoleIdAsync(string roleName, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByNameAsync(roleName, cancellationToken);
        if (role is not null)
        {
            return role.Id;
        }

        _logger.LogWarning("Role {RoleName} does not exist. Creating requested role", roleName);

        var newRole = new Role
        {
            Name = roleName
        };

        var newRoleId = await _roleRepository.CreateAsync(newRole, cancellationToken);
        if (newRoleId == Guid.Empty)
        {
            throw new RoleNotCreatedException($"Failed to create role {roleName}");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return newRoleId;
    }
}
