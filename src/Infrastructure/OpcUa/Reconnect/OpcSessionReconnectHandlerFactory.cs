using OMP.Connector.Domain.OpcUa.Services;
using OMP.Connector.Infrastructure.OpcUa.Reconnect;

namespace OMP.Connector.Infrastructure.OpcUa.Reconnect
{
    public class OpcSessionReconnectHandlerFactory : IOpcSessionReconnectHandlerFactory
    {
        private readonly ISubscriptionRestoreService _subscriptionRestoreService;

        public OpcSessionReconnectHandlerFactory(ISubscriptionRestoreService subscriptionRestoreService)
        {
            this._subscriptionRestoreService = subscriptionRestoreService;
        }

        public IOpcSessionReconnectHandler Create()
            => new OpcSessionReconnectHandler(this._subscriptionRestoreService);
    }
}