// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using ApplicationV2.Models.Subscriptions;
using Microsoft.Extensions.Options;
using ApplicationV2.Configuration;
using System.Collections.Concurrent;

namespace ApplicationV2.Repositories
{
    public class SubscriptionRepositoryInMemory : ISubscriptionRepository
    {
        private ConnectorConfiguration connectorConfiguration;
        private ConcurrentDictionary<string, List<SubscriptionDto>> pairs = new ConcurrentDictionary<string, List<SubscriptionDto>>();

        public SubscriptionRepositoryInMemory(IOptions<ConnectorConfiguration> options)
        {
            connectorConfiguration = options.Value;
        }


        public void Add(string endpointUrl, IEnumerable<SubscriptionMonitoredItem> monitoredItems)
        {
            if (connectorConfiguration.DisableSubscriptionRestoreService)
                return;

            var monitoredItemsDict = monitoredItems.ToDictionary(monitoredItem => monitoredItem.NodeId);
            var newSubscriptionDto = new SubscriptionDto
            {
                EndpointUrl = endpointUrl,
                MonitoredItems = monitoredItemsDict
            };

            if(pairs.ContainsKey(endpointUrl))
            {
                var subscriptions = pairs[endpointUrl];
                subscriptions.Add(newSubscriptionDto);
            }

            if(pairs.TryAdd(endpointUrl, new List<SubscriptionDto> { newSubscriptionDto }))
                return;

            //TODO: raise some kin of error
        }
    }
}
