using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Y.Authentication.Domain.Repositories;
using Y.Authentication.Infrastructure.Persistence;
using Y.Authentication.Infrastructure.Persistence.Repositories;
using Y.Contract.Root.Notification.Events;

namespace Y.Authentication.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddPersistence(configuration)
            .AddRepositories()
            .AddRabbitMQ(configuration);
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
        services.AddScoped<IUserMetadataRepository, UserMetadataRepository>();

        return services;
    }

    private static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(registration =>
        {
            registration.SetKebabCaseEndpointNameFormatter();

            var host = configuration["RabbitMQ:Host"]!;
            var username = configuration["RabbitMQ:Username"]!;
            var password = configuration["RabbitMQ:Password"]!;

            registration.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(host, hostConfiguration =>
                {
                    hostConfiguration.Username(username);
                    hostConfiguration.Password(password);
                });
                cfg.ConfigureEndpoints(context);

                cfg.Message<SendEmailEvent>(topology => topology.SetEntityName(SendEmailEvent.Exchange));
                cfg.Publish<SendEmailEvent>(topology => topology.ExchangeType = "direct");
            });
        });

        return services;
    }
}
