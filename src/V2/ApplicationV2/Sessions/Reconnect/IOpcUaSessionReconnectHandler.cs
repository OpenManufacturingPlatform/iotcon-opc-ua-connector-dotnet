// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua.Client;

namespace ApplicationV2.Sessions.Reconnect
{
    public interface IOpcUaSessionReconnectHandler : IDisposable
    {
        void BeginReconnect(IOpcUaSession opcUaSession, Session session, int reconnectPeriod, EventHandler callback);
        bool IsHealthy { get; }
    }
}
