using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.Factories;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.OpcUa.Services;

namespace OMP.Connector.Application.Services
{
    public class SubscriptionServiceFactory : ISubscriptionServiceFactory
    {
        private readonly ISessionPoolStateManager _sessionPoolStateManager;
        private readonly IOptions<ConnectorConfiguration> _connectorConfiguration;
        private readonly ISubscriptionProviderFactory _subscriptionProviderFactory;
        private readonly ILoggerFactory _loggerFactory;

        public SubscriptionServiceFactory(
            ISessionPoolStateManager sessionPoolStateManager,
            IOptions<ConnectorConfiguration> connectorConfiguration,
            ISubscriptionProviderFactory subscriptionProviderFactory,
            ILoggerFactory loggerFactory
        )
        {
            this._sessionPoolStateManager = sessionPoolStateManager;
            this._connectorConfiguration = connectorConfiguration;
            this._subscriptionProviderFactory = subscriptionProviderFactory;
            this._loggerFactory = loggerFactory;
        }

        public ISubscriptionService Create()
            => new SubscriptionService(
                this._sessionPoolStateManager,
                this._connectorConfiguration,
                this._subscriptionProviderFactory,
                this._loggerFactory.CreateLogger<SubscriptionService>()
            );
    }
}