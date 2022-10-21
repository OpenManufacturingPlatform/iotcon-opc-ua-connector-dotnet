// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Microsoft.Extensions.Options;
using OMP.PlantConnectivity.OpcUa.Configuration;
using System.Collections.Concurrent;
using OMP.PlantConnectivity.OpcUa.Models.Alarms;

namespace OMP.PlantConnectivity.OpcUa.Repositories
{
    public sealed class AlarmSubscriptionRepositoryInMemory : IAlarmSubscriptionRepository
    {
        private OmpOpcUaConfiguration connectorConfiguration;
        private ConcurrentDictionary<string, List<AlarmSubscriptionDto>> pairs = new ConcurrentDictionary<string, List<AlarmSubscriptionDto>>();

        public AlarmSubscriptionRepositoryInMemory(IOptions<OmpOpcUaConfiguration> options)
        {
            connectorConfiguration = options.Value;
        }


        public void Add(string endpointUrl, IEnumerable<AlarmSubscriptionMonitoredItem> monitoredItems)
        {
            if (connectorConfiguration.DisableSubscriptionRestoreService)
                return;

            var monitoredItemsDict = monitoredItems.ToDictionary(monitoredItem => monitoredItem.NodeId);
            var newSubscriptionDto = new AlarmSubscriptionDto
            {
                EndpointUrl = endpointUrl,
                AlarmMonitoredItems = monitoredItemsDict
            };

            if (pairs.ContainsKey(endpointUrl))
            {
                var subscriptions = pairs[endpointUrl];
                subscriptions.Add(newSubscriptionDto);
            }

            if (pairs.TryAdd(endpointUrl, new List<AlarmSubscriptionDto> { newSubscriptionDto }))
                return;

            //TODO: raise some kind of error
        }

        public void DeleteAlarmMonitoredItems(string endpointUrl, IEnumerable<string> nodeIds)
        {
            if (connectorConfiguration.DisableAlarmSubscriptionRestoreService)
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
                                    .AlarmMonitoredItems
                                    .Remove(nodeId);

                    if (removed)
                        nodeIdList.Remove(nodeId);

                    counter++;
                }
            }
        }

        public IEnumerable<AlarmSubscriptionDto> GetAllByEndpointUrl(string endpointUrl)
        {
            if (connectorConfiguration.DisableSubscriptionRestoreService)
                return Array.Empty<AlarmSubscriptionDto>();

            if (!pairs.ContainsKey(endpointUrl))
                return pairs[endpointUrl];

            return Array.Empty<AlarmSubscriptionDto>();
        }

        public bool Remove(AlarmSubscriptionDto subscription)
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
