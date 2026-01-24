using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Y.Authentication.Domain.DomainEvents.Base;
using Y.Authentication.Domain.Options;
using Y.Authentication.Domain.Repositories;
using Y.Authentication.Domain.Services;
using Y.Authentication.Infrastructure.Background;
using Y.Authentication.Infrastructure.DomainEvents;
using Y.Authentication.Infrastructure.Messasing;
using Y.Authentication.Infrastructure.Persistence;
using Y.Authentication.Infrastructure.Persistence.Repositories;
using Y.Authentication.Infrastructure.Resilience;
using Y.Authentication.Infrastructure.Services;

namespace Y.Authentication.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddOptions(configuration)
            .AddPersistence(configuration)
            .AddRepositories()
            .AddBackgroundServices()
            .AddServices()
            .AddDomainEventsDispatcher()
            .AddPipelinePolicies()
            .ConfigureKafkaTopology(configuration);
    }

    public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuthOptions>(c => configuration.GetSection("Jwt").Bind(c));

        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DatabaseConnection");

        services.AddDbContext<AppDataContext>(options => options.UseSqlServer(connectionString));
        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();

        return services;
    }

    private static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<RoleConfiguratorBackgroundService>();

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IProducerService, ProducerService>();

        return services;
    }

    private static IServiceCollection AddDomainEventsDispatcher(this IServiceCollection services)
    {
        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();
        return services;
    }

    private static IServiceCollection AddPipelinePolicies(this IServiceCollection services)
    {
        services.AddResiliencePipeline(Resiliences.FastDefaultRetryPipelinePolicy, builder => ResilienceBuilder.FastDefaultRetryPipelinePolicy(builder));

        return services;
    }
}
