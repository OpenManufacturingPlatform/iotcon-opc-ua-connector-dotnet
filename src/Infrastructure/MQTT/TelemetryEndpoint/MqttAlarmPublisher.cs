// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Schema.Alarms;
using OMP.Connector.Infrastructure.MQTT.Common;
using OMP.Connector.Infrastructure.MQTT.Common.Publishers;
using OMP.Connector.Infrastructure.MQTT.Serialization;

namespace OMP.Connector.Infrastructure.MQTT.ResponseEndpoint
{
    public class MqttAlarmPublisher : MqttBaseListnerPublisher<IMqttClient, MqttClientSettings>, IMqttAlarmPublisher
    {
        public MqttAlarmPublisher(
             IOptions<ConnectorConfiguration> connectorConfiguration,
             IMqttClientFactory mqttClientFactory,
             ISerializer serializer,
             ILogger<IMqttClient> logger)
             : base(mqttClientFactory.CreateClient(connectorConfiguration.Value.Communication.AlarmEndpoint, connectorConfiguration.Value.Communication.Shared),
                  connectorConfiguration.Value.Communication.AlarmEndpoint.GetConfig<MqttClientSettings>(),
                  serializer,
                  logger,
                  connectorConfiguration.Value.Communication.AlarmEndpoint.GetConfig<MqttClientSettings>().AutoReconnectTimeInSeconds)
        { }

        public Task PublishAsync(AlarmMessage message)
            => base.PublishAsync(message);
    }
}
