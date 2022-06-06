// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Application.Factories;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.OpcUa.Services;

namespace OMP.Connector.Application.Services
{
    public class AlarmSubscriptionServiceFactory : IAlarmSubscriptionServiceFactory
    {
        private readonly ISessionPoolStateManager _sessionPoolStateManager;
        private readonly IOptions<ConnectorConfiguration> _connectorConfiguration;
        private readonly IAlarmSubscriptionProviderFactory _subscriptionProviderFactory;
        private readonly ILoggerFactory _loggerFactory;

        public AlarmSubscriptionServiceFactory(
            ISessionPoolStateManager sessionPoolStateManager,
            IOptions<ConnectorConfiguration> connectorConfiguration,
            IAlarmSubscriptionProviderFactory subscriptionProviderFactory,
            ILoggerFactory loggerFactory
        )
        {
            this._sessionPoolStateManager = sessionPoolStateManager;
            this._connectorConfiguration = connectorConfiguration;
            this._subscriptionProviderFactory = subscriptionProviderFactory;
            this._loggerFactory = loggerFactory;
        }

        public IAlarmSubscriptionService Create()
            => new AlarmSubscriptionService(
                this._sessionPoolStateManager,
                this._connectorConfiguration,
                this._subscriptionProviderFactory,
                this._loggerFactory.CreateLogger<AlarmSubscriptionService>()
            );
    }
}
