using OMP.Connector.Domain.OpcUa.Services;

namespace OMP.Connector.Infrastructure.Kafka.Reconnect
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