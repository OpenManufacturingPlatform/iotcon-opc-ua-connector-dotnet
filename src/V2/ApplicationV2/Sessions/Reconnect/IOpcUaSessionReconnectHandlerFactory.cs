// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUA.Sessions.Reconnect
{
    public interface IOpcUaSessionReconnectHandlerFactory
    {
        IOpcUaSessionReconnectHandler Create();
    }
}
