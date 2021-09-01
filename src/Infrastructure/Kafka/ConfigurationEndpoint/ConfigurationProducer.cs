using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using OMP.Connector.Domain.Models;
using OMP.Device.Connector.Kafka.Common.Configuration;
using OMP.Device.Connector.Kafka.Common.Events;
using OMP.Device.Connector.Kafka.Common.Producers;
using OMP.Device.Connector.Kafka.Common.Producers.Responses;
using OMP.Device.Connector.Kafka.Serialization;
using OneOf;

namespace OMP.Device.Connector.Kafka.ConfigurationEndpoint
{
    public class ConfigurationProducer : CustomKafkaProducer<string, AppConfigDto>, IConfigurationPersister, IConfigurationProducer
    {
        public ConfigurationProducer(
            KafkaConfig kafkaConfig,
            ProducerConfig configuration,
            ILogger<ConfigurationProducer> logger,
            ISerializerFactory serializerFactory,
            IKafkaEventHandlerFactory kafkaEventHandlerFactory)
            : base(kafkaConfig, configuration, logger, serializerFactory, kafkaEventHandlerFactory)
        { }

        public Task<OneOf<PublishSucceeded, PublishPartialSucceeded, PublishedFailedMessageSizeTooLarge,PublishFailed>> SaveConfigurationAsync(AppConfigDto appConfig, CancellationToken cancellationToken)
            => this.ProduceAsync(appConfig, cancellationToken);
    }
}
