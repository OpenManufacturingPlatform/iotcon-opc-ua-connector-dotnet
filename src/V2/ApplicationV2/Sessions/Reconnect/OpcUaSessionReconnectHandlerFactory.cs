// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUA.Sessions.Subscriptions;

namespace OMP.PlantConnectivity.OpcUA.Sessions.Reconnect
{
    internal class OpcUaSessionReconnectHandlerFactory : IOpcUaSessionReconnectHandlerFactory
    {
        public IOpcUaSessionReconnectHandler Create()
            => new OpcUaSessionReconnectHandler();
    }
}
