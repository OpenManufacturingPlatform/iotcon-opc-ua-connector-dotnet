// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.Connector.Domain.OpcUa.Services;

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