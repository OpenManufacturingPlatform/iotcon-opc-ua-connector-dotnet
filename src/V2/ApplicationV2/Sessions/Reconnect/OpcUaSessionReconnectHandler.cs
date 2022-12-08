// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUa.Sessions.Subscriptions;
using Opc.Ua.Client;

namespace OMP.PlantConnectivity.OpcUa.Sessions.Reconnect
{
    internal sealed class OpcUaSessionReconnectHandler : IOpcUaSessionReconnectHandler
    {
        private SessionReconnectHandler? reconnectHandler;
        private EventHandler? callback;
        private IOpcUaSession? opcUaSession;

        public bool IsHealthy => this.reconnectHandler != default;

        public void BeginReconnect(IOpcUaSession opcUaSession, Session session, int reconnectPeriod, EventHandler callback)
        {
            if (IsHealthy)
                return;

            this.opcUaSession = opcUaSession;
            this.callback = callback;
            reconnectHandler = new SessionReconnectHandler();
            reconnectHandler.BeginReconnect(session, reconnectPeriod, ServerReconnectComplete!);
        }

        public void Dispose()
        {
            this.reconnectHandler?.Dispose();
        }

        private void ServerReconnectComplete(object sender, EventArgs e)
        {
            if (!ReferenceEquals(sender, this.reconnectHandler))
                return;

            this.reconnectHandler?.Dispose();
            this.reconnectHandler = null;

            //OPCFoundation stack handles the transfer of subscriptions to a new session on reconnect
            opcUaSession?.RestoreRegisteredNodeIds();            

            this.callback?.Invoke(sender, e);
        }
    }
}
