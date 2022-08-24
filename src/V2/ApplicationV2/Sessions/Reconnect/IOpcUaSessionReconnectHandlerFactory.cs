// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Sessions.Reconnect
{
    public interface IOpcUaSessionReconnectHandlerFactory
    {
        IOpcUaSessionReconnectHandler Create();
    }
}
