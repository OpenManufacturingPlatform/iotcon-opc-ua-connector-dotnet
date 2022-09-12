// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUA.Models.Subscriptions;
using Microsoft.Extensions.Options;
using OMP.PlantConnectivity.OpcUA.Configuration;
using System.Collections.Concurrent;

namespace OMP.PlantConnectivity.OpcUA.Repositories
{
    internal class SubscriptionRepositoryInMemory : ISubscriptionRepository
    {
        private OmpOpcUaConfiguration connectorConfiguration;
        private ConcurrentDictionary<string, List<SubscriptionDto>> pairs = new ConcurrentDictionary<string, List<SubscriptionDto>>();

        public SubscriptionRepositoryInMemory(IOptions<OmpOpcUaConfiguration> options)
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

            if (pairs.ContainsKey(endpointUrl))
            {
                var subscriptions = pairs[endpointUrl];
                subscriptions.Add(newSubscriptionDto);
            }

            if (pairs.TryAdd(endpointUrl, new List<SubscriptionDto> { newSubscriptionDto }))
                return;

            //TODO: raise some kind of error
        }

        public void DeleteMonitoredItems(string endpointUrl, IEnumerable<string> nodeIds)
        {
            if (connectorConfiguration.DisableSubscriptionRestoreService)
                return;

            if (!pairs.ContainsKey(endpointUrl))
                return;

            var currentMonitoredItems = pairs[endpointUrl];
            var nodeIdList = nodeIds.ToList();
            foreach (var subscription in currentMonitoredItems)
            {
                var counter = 0;
                while (counter < nodeIdList.Count)
                {
                    var nodeId = nodeIdList[counter];
                    var removed = subscription
                                    .MonitoredItems
                                    .Remove(nodeId);

                    if (removed)
                        nodeIdList.Remove(nodeId);

                    counter++;
                }
            }
        }

        public IEnumerable<SubscriptionDto> GetAllByEndpointUrl(string endpointUrl)
        {
            if (connectorConfiguration.DisableSubscriptionRestoreService)
                return Array.Empty<SubscriptionDto>();

            if (!pairs.ContainsKey(endpointUrl))
                return pairs[endpointUrl];

            return Array.Empty<SubscriptionDto>();
        }

        public bool Remove(SubscriptionDto subscription)
        {
            if (!pairs.ContainsKey(subscription.EndpointUrl))
                return true;

            var subscriptions = pairs[subscription.EndpointUrl];
            subscriptions.Remove(subscription);

            pairs[subscription.EndpointUrl] = subscriptions;

            return true;
        }
    }
}
