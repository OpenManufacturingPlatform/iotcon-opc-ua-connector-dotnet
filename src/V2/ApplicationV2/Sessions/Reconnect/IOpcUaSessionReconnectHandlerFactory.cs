// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUa.Sessions.Reconnect
{
    public interface IOpcUaSessionReconnectHandlerFactory
    {
        IOpcUaSessionReconnectHandler Create();
    }
}
