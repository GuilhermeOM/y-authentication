using KafkaFlow.Producers;
using Polly.Registry;
using Y.Authentication.Domain.Constants;
using Y.Authentication.Domain.Services;
using Y.Authentication.Infrastructure.Resilience;
using Y.Contract.SharedKernel.Abstractions.Messaging;

namespace Y.Authentication.Infrastructure.Services;
internal sealed class ProducerService : IProducerService
{
    private readonly IProducerAccessor _producerAccessor;
    private readonly ResiliencePipelineProvider<string> _resiliencePipelineProvider;

    public ProducerService(
        IProducerAccessor producerAccessor,
        ResiliencePipelineProvider<string> resiliencePipelineProvider)
    {
        _producerAccessor = producerAccessor;
        _resiliencePipelineProvider = resiliencePipelineProvider;
    }

    public async Task ProduceAsync(IKafkaMessage message, MessageMetadata metadata)
    {
        await _resiliencePipelineProvider
            .GetPipeline(Resiliences.FastDefaultRetryPipelinePolicy)
            .ExecuteAsync(async _ =>
            {
                await _producerAccessor[KafkaConstants.Producers.Authentication]
                    .ProduceAsync(metadata.Topic, metadata.MessageKey, message);
            });
    }
}
