// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua.Client;

namespace OMP.PlantConnectivity.OpcUa.Sessions.Reconnect
{
    public interface IOpcUaSessionReconnectHandler : IDisposable
    {
        void BeginReconnect(IOpcUaSession opcUaSession, Session session, int reconnectPeriod, EventHandler callback);
        bool IsHealthy { get; }
    }
}
