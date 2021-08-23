using Confluent.Kafka;
using Microsoft.Extensions.Options;
using OMP.Connector.Domain.Configuration;
using OMP.Device.Connector.Kafka.CommandEndpoint;
using OMP.Device.Connector.Kafka.Common.Configuration;
using OMP.Device.Connector.Kafka.Common.Events;
using OMP.Device.Connector.Kafka.ConfigurationEndpoint;
using OMP.Device.Connector.Kafka.Serialization;

namespace OMP.Device.Connector.Kafka.Common.Consumers
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
            if (_connectorConfiguration.Persistance.Type != CommunicationType.Kafka)
                return null;

            ConsumerConfig sharedKafkaConfig = null;
            if (_connectorConfiguration.Communication.Shared.Type == CommunicationType.Kafka)
                sharedKafkaConfig = _connectorConfiguration.Communication.Shared.GetConfig<ConsumerConfig>();

            var (kafkaConfiguration, consumerConfig) = GetEndpointConfiguration(_connectorConfiguration.Persistance);

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
