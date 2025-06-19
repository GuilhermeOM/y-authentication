using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Y.Authentication.Application.Abstractions.Messaging;

namespace Y.Authentication.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddUseCases();
    }

    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        return services.Scan(scan => scan.FromAssembliesOf(typeof(DependencyInjection))
            .AddClasses(classes => classes.AssignableToAny(
                typeof(IUseCaseHandler<>),
                typeof(IUseCaseHandler<,>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());
    }
}
