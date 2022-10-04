// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUa.Sessions.Subscriptions;

namespace OMP.PlantConnectivity.OpcUa.Sessions.Reconnect
{
    internal sealed class OpcUaSessionReconnectHandlerFactory : IOpcUaSessionReconnectHandlerFactory
    {
        public IOpcUaSessionReconnectHandler Create()
            => new OpcUaSessionReconnectHandler();
    }
}
