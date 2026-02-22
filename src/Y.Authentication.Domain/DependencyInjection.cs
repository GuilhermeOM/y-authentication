using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Y.Authentication.Domain.Options;

namespace Y.Authentication.Domain;
public static class DependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddOptions(configuration);
    }

    public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AuthOptions>().Bind(configuration.GetSection("Jwt"));
        services.AddOptions<BlobStorageOptions>().Bind(configuration.GetSection("Options:BlobStorage"));

        return services;
    }
}
