// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using OMP.Connector.Domain.OpcUa;
using Opc.Ua.Client;

namespace OMP.Connector.Infrastructure.OpcUa.Reconnect
{
    public interface IOpcSessionReconnectHandler : IDisposable
    {
        void BeginReconnect(IOpcSession opcSession, Session session, int reconnectPeriod, IRegisteredNodeStateManager registeredNodeStateManager, EventHandler callback);
        bool IsHealthy { get; }
    }
}