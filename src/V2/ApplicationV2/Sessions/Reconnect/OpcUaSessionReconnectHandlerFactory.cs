// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using ApplicationV2.Sessions.Subscriptions;

namespace ApplicationV2.Sessions.Reconnect
{
    internal class OpcUaSessionReconnectHandlerFactory : IOpcUaSessionReconnectHandlerFactory
    {
        public IOpcUaSessionReconnectHandler Create()
            => new OpcUaSessionReconnectHandler();
    }
}
