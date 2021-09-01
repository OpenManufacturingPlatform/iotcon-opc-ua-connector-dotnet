using System;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.OpcUa.Services;
using OMP.Connector.Infrastructure.OpcUa.Reconnect;
using Opc.Ua.Client;

namespace OMP.Connector.Infrastructure.OpcUa.Reconnect
{
    public class OpcSessionReconnectHandler : IOpcSessionReconnectHandler
    {
        private readonly ISubscriptionRestoreService _subscriptionRestoreService;
        private Session _session;
        private SessionReconnectHandler _reconnectHandler;
        private EventHandler _callback;
        private IOpcSession _opcSession;
        private IRegisteredNodeStateManager _registeredNodeStateManager;

        internal OpcSessionReconnectHandler(ISubscriptionRestoreService subscriptionRestoreService)
        {
            this._subscriptionRestoreService = subscriptionRestoreService;
        }

        public bool IsHealthy => this._reconnectHandler != default;

        public void BeginReconnect(IOpcSession opcSession, Session session, int reconnectPeriod, IRegisteredNodeStateManager registeredNodeStateManager, EventHandler callback)
        {
            if (this.IsHealthy)
                return;

            this._opcSession = opcSession;
            this._registeredNodeStateManager = registeredNodeStateManager;
            this._session = session;
            this._callback = callback;
            this._reconnectHandler = new SessionReconnectHandler();
            this._reconnectHandler.BeginReconnect(session, reconnectPeriod, this.ServerReconnectComplete);
        }

        public void Dispose()
        {
            this._reconnectHandler?.Dispose();
        }

        private void ServerReconnectComplete(object sender, EventArgs e)
        {
            if (!ReferenceEquals(sender, this._reconnectHandler))
                return;

            this._session = this._reconnectHandler?.Session;
            this._reconnectHandler?.Dispose();
            this._reconnectHandler = null;
            this._subscriptionRestoreService.RestoreSubscriptionsAsync(this._opcSession).GetAwaiter().GetResult();
            this._registeredNodeStateManager?.RestoreRegisteredNodeIds(this._session);

            this._callback?.Invoke(sender, e);
        }
    }
}
