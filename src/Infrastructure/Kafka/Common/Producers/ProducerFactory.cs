// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Infrastructure.Kafka.Common.Configuration;
using OMP.Connector.Infrastructure.Kafka.Common.Events;
using OMP.Connector.Infrastructure.Kafka.ConfigurationEndpoint;
using OMP.Connector.Infrastructure.Kafka.ResponsesEndpoint;
using OMP.Connector.Infrastructure.Kafka.Serialization;
using OMP.Connector.Infrastructure.Kafka.TelemetryEndpoint;

namespace OMP.Connector.Infrastructure.Kafka.Common.Producers
{
    public class ProducerFactory : IProducerFactory
    {
        private readonly ConnectorConfiguration _connectorConfiguration;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ISerializerFactory _serializerFactory;
        private readonly IKafkaEventHandlerFactory _kafkaEventHandlerFactory;

        public ProducerFactory(
            IOptions<ConnectorConfiguration> connectorConfiguration,
            ILoggerFactory loggerFactory,
            ISerializerFactory serializerFactory,
            IKafkaEventHandlerFactory kafkaEventHandlerFactory = null)
        {
            _connectorConfiguration = connectorConfiguration.Value;
            _loggerFactory = loggerFactory;
            _serializerFactory = serializerFactory;
            _kafkaEventHandlerFactory = kafkaEventHandlerFactory;
        }

        public IConfigurationProducer CreateConfigurationProducer()
        {
            if (_connectorConfiguration?.Persistence?.Type != CommunicationType.Kafka)
                return null;

            ProducerConfig sharedKafkaConfig = null;
            if (_connectorConfiguration.Communication.Shared.Type == CommunicationType.Kafka)
                sharedKafkaConfig = _connectorConfiguration.Communication.Shared.GetConfig<ProducerConfig>();

            var (kafkaConfiguration, producerConfig) = GetEndpointConfiguration(_connectorConfiguration.Persistence);

            if (sharedKafkaConfig != null)
            {
                foreach (var (key, value) in producerConfig)
                    sharedKafkaConfig.Set(key, value);
            }

            return new ConfigurationProducer(
                kafkaConfiguration,
                sharedKafkaConfig,
                _loggerFactory.CreateLogger<ConfigurationProducer>(),
                _serializerFactory,
                _kafkaEventHandlerFactory);
        }

        public IResponseProducer CreateResponseProducer()
        {
            if (_connectorConfiguration?.Communication?.ResponseEndpoint?.Type != CommunicationType.Kafka)
                return null;

            var (kafkaConfiguration, producerConfig) = GetEndpointConfiguration(_connectorConfiguration.Communication.ResponseEndpoint);

            return new ResponseProducer(
                kafkaConfiguration,
                producerConfig,
                _loggerFactory.CreateLogger<ResponseProducer>(),
                _serializerFactory,
                _kafkaEventHandlerFactory);
        }

        public ITelemetryProducer CreateTelemetryProducer()
        {
            if (_connectorConfiguration?.Communication?.TelemetryEndpoint?.Type != CommunicationType.Kafka)
                return null;

            var (kafkaConfiguration, producerConfig) = GetEndpointConfiguration(_connectorConfiguration.Communication.TelemetryEndpoint);

            return new TelemetryProducer(
                kafkaConfiguration,
                producerConfig,
                _loggerFactory.CreateLogger<TelemetryProducer>(),
                _serializerFactory,
                _kafkaEventHandlerFactory);
        }


        private (KafkaConfig kafkaConfiguration, ProducerConfig consumerConfig) GetEndpointConfiguration(CommunicationChannelConfiguration endpointConfiguration)
            => (endpointConfiguration.GetConfig<KafkaConfig>(), endpointConfiguration.GetConfig<ProducerConfig>());
    }
}
