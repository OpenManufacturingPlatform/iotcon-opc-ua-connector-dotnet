// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using OMP.Connector.Domain.Models;
using OMP.Connector.Infrastructure.Kafka.Common.Configuration;
using OMP.Connector.Infrastructure.Kafka.Common.Events;
using OMP.Connector.Infrastructure.Kafka.Common.Producers;
using OMP.Connector.Infrastructure.Kafka.Common.Producers.Responses;
using OMP.Connector.Infrastructure.Kafka.Serialization;
using OneOf;

namespace OMP.Connector.Infrastructure.Kafka.ConfigurationEndpoint
{
    public class ConfigurationProducer : CustomKafkaProducer<string, AppConfigDto>, IConfigurationPersister, IConfigurationProducer
    {
        public ConfigurationProducer(
            KafkaConfig kafkaConfig,
            ProducerConfig configuration,
            ILogger<ConfigurationProducer> logger,
            ISerializerFactory serializerFactory,
            IKafkaEventHandlerFactory kafkaEventHandlerFactory = null)
            : base(kafkaConfig, configuration, logger, serializerFactory, kafkaEventHandlerFactory)
        { }

        public Task<OneOf<PublishSucceeded, PublishPartialSucceeded, PublishedFailedMessageSizeTooLarge, PublishFailed>> SaveConfigurationAsync(AppConfigDto appConfig, CancellationToken cancellationToken)
            => this.ProduceAsync(appConfig, cancellationToken);
    }
}
