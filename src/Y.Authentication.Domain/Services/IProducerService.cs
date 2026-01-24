using Y.Contract.SharedKernel.Abstractions.Messaging;

namespace Y.Authentication.Domain.Services;
public interface IProducerService
{
    Task ProduceAsync(IKafkaMessage message, MessageMetadata metadata);
}
