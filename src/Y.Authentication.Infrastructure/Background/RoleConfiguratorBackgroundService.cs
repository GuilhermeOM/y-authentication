using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Y.Authentication.Domain.Repositories;

namespace Y.Authentication.Infrastructure.Background;
internal sealed class RoleConfiguratorBackgroundService : IHostedService
{
    private const int UniqueConstraintViolationSqlErrorCode = 2601;

    private readonly ILogger<RoleConfiguratorBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public RoleConfiguratorBackgroundService(
        ILogger<RoleConfiguratorBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting roles configuration");

        try
        {
            using var scope = _serviceProvider.CreateAsyncScope();
            var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await roleRepository.CreateManyAsync(
            [
                Domain.Aggregates.Role.Role.Create(Contract.SharedKernel.Enums.Role.Admin.ToString()).Value,
                Domain.Aggregates.Role.Role.Create(Contract.SharedKernel.Enums.Role.User.ToString()).Value,
                Domain.Aggregates.Role.Role.Create(Contract.SharedKernel.Enums.Role.SuperUser.ToString()).Value,
            ], cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlException && sqlException.Number == UniqueConstraintViolationSqlErrorCode)
        {
            _logger.LogInformation("Roles are already configured");
        }

        _logger.LogInformation("Roles configuration successfully completed");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
