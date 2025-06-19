using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Y.Authentication.Application.Abstractions.Behaviors;
using Y.Authentication.Application.Abstractions.Messaging;

namespace Y.Authentication.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddUseCases()
            .AddDecorators();
    }

    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.Scan(scan => scan.FromAssembliesOf(typeof(DependencyInjection))
            .AddClasses(classes => classes.AssignableTo(typeof(IUseCaseHandler<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IUseCaseHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }

    public static IServiceCollection AddDecorators(this IServiceCollection services)
    {
        services.TryDecorate(typeof(IUseCaseHandler<>), typeof(LoggingDecorator.UseCaseHandler<>));
        services.TryDecorate(typeof(IUseCaseHandler<,>), typeof(LoggingDecorator.UseCaseHandler<,>));

        return services;
    }
}
