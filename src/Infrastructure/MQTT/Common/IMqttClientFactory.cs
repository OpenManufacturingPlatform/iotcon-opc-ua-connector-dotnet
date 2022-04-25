// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.Connector.Domain.Configuration;

namespace OMP.Connector.Infrastructure.MQTT.Common
{
    public interface IMqttClientFactory
    {
        IMqttClient CreateClient(CommunicationChannelConfiguration channelConfiguration, SharedConfiguration sharedConfiguration);
    }
}
