// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Schema.SensorTelemetry;
using OMP.Connector.Infrastructure.MQTT.Common;
using OMP.Connector.Infrastructure.MQTT.Common.Publishers;
using OMP.Connector.Infrastructure.MQTT.Serialization;

namespace OMP.Connector.Infrastructure.MQTT.ResponseEndpoint
{
    public class MqttTelemetryPublisher : MqttBaseListnerPublisher<IMqttClient, MqttClientSettings>, IMqttTelemetryPublisher
    {
        public MqttTelemetryPublisher(
             IOptions<ConnectorConfiguration> connectorConfiguration,
             IMqttClientFactory mqttClientFactory,
             ISerializer serializer,
             ILogger<IMqttClient> logger)
             : base(mqttClientFactory.CreateClient(connectorConfiguration.Value.Communication.TelemetryEndpoint, connectorConfiguration.Value.Communication.Shared),
                  connectorConfiguration.Value.Communication.TelemetryEndpoint.GetConfig<MqttClientSettings>(),
                  serializer,
                  logger,
                  connectorConfiguration.Value.Communication.TelemetryEndpoint.GetConfig<MqttClientSettings>().AutoReconnectTimeInSeconds)
        { }

        public Task PublishAsync(SensorTelemetryMessage message)
            => base.PublishAsync(message);
    }
}
