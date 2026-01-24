using KafkaFlow;
using KafkaFlow.Configuration;
using KafkaFlow.Serializer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Y.Authentication.Domain.Constants;

namespace Y.Authentication.Infrastructure.Messasing;
internal static class KafkaTopologyConfiguration
{
    public static IServiceCollection ConfigureKafkaTopology(this IServiceCollection services, IConfiguration configuration)
    {
        var brokers = configuration.GetRequiredSection("Kafka:Brokers").Get<string[]>();

        services.AddKafka(kafka => kafka
            .UseMicrosoftLog()
            .AddCluster(cluster => cluster
                .WithBrokers(brokers)
                .ConfigureProducers()
            )
        );

        return services;
    }

    public static IClusterConfigurationBuilder ConfigureProducers(this IClusterConfigurationBuilder cluster)
    {
        return cluster
            .AddProducer(KafkaConstants.Producers.Authentication, producer => producer
                .AddMiddlewares(middlewares => middlewares.AddSerializer<JsonCoreSerializer>())
            );
    }
}
