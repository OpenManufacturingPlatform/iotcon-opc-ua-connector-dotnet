using Confluent.Kafka;
using Microsoft.Extensions.Options;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Infrastructure.Kafka.CommandEndpoint;
using OMP.Connector.Infrastructure.Kafka.Common.Configuration;
using OMP.Connector.Infrastructure.Kafka.Common.Events;
using OMP.Connector.Infrastructure.Kafka.ConfigurationEndpoint;
using OMP.Connector.Infrastructure.Kafka.Serialization;

namespace OMP.Connector.Infrastructure.Kafka.Common.Consumers
{
    public class ConsumerFactory : IConsumerFactory
    {
        private readonly ConnectorConfiguration _connectorConfiguration;
        private readonly ISerializerFactory _serializerFactory;
        private readonly IKafkaEventHandlerFactory _kafkaEventHandlerFactory;

        public ConsumerFactory(
            IOptions<ConnectorConfiguration> connectorConfiguration,
            ISerializerFactory serializerFactory,
            IKafkaEventHandlerFactory kafkaEventHandlerFactory)
        {
            _connectorConfiguration = connectorConfiguration.Value;
            _serializerFactory = serializerFactory;
            _kafkaEventHandlerFactory = kafkaEventHandlerFactory;
        }

        public ICommandConsumer CreateCommandConsumer()
        {
            if (_connectorConfiguration.Communication.CommandEndpoint.Type != CommunicationType.Kafka)
                return null;

            var (kafkaConfiguration, consumerConfig) = GetEndpointConfiguration(_connectorConfiguration.Communication.CommandEndpoint);
            return new CommandConsumer(
                kafkaConfiguration,
                consumerConfig,
                _serializerFactory,
                _kafkaEventHandlerFactory);
        }

        public IConfigurationConsumer CreateConfigurationConsumer()
        {
            if (_connectorConfiguration.Persistence.Type != CommunicationType.Kafka)
                return null;

            ConsumerConfig sharedKafkaConfig = null;
            if (_connectorConfiguration.Communication.Shared.Type == CommunicationType.Kafka)
                sharedKafkaConfig = _connectorConfiguration.Communication.Shared.GetConfig<ConsumerConfig>();

            var (kafkaConfiguration, consumerConfig) = GetEndpointConfiguration(_connectorConfiguration.Persistence);

            if (sharedKafkaConfig != null)
            {
                foreach (var (key, value) in consumerConfig)
                    sharedKafkaConfig.Set(key, value);
            }

            return new ConfigurationConsumer(
                kafkaConfiguration,
                sharedKafkaConfig,
                _serializerFactory,
                _kafkaEventHandlerFactory);
        }

        private (KafkaConfig kafkaConfiguration, ConsumerConfig consumerConfig) GetEndpointConfiguration(CommunicationChannelConfiguration endpointConfiguration)
            => (endpointConfiguration.GetConfig<KafkaConfig>(), endpointConfiguration.GetConfig<ConsumerConfig>());
    }
}
