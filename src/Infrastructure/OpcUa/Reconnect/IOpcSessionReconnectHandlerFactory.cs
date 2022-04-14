// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.Connector.Infrastructure.OpcUa.Reconnect
{
    public interface IOpcSessionReconnectHandlerFactory
    {
        IOpcSessionReconnectHandler Create();
    }
}