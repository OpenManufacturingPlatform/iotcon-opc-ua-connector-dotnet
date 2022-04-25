// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.Connector.Infrastructure.MQTT.Common
{
    public struct PublishArguments
    {
        public byte[] Data;
        public string Topic;
        public byte QosLevel;
        public bool Retain;
    }
}
