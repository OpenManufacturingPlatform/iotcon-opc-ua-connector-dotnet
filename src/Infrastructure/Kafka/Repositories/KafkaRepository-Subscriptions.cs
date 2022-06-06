// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Generic;
using System.Linq;
using OMP.Connector.Domain.Models;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Schema;

namespace OMP.Connector.Infrastructure.Kafka.Repositories
{

    public partial class KafkaRepository : ISubscriptionRepository
    {
        public bool Add(SubscriptionDto subscription)
        {
            WaitForInitializationToComplete();

            var persisted = false;
            var cacheChanged = GroupByPublishingInterval(subscription)
                            .Aggregate(false,
                                (current, normalizedSubscription) =>
                                    this.UpdateSubscriptionCache(normalizedSubscription, true) || current
                                );

            if (cacheChanged)
                persisted = PersistCachedConfig();

            return (cacheChanged && persisted) || (!cacheChanged);
        }

        public bool DeleteMonitoredItems(string endpointUrl, IEnumerable<OpcUaMonitoredItem> items)
        {
            WaitForInitializationToComplete();

            var cacheChanged = RemoveMonitoredItemsFromCache(endpointUrl, items);

            var persisted = false;
            if (cacheChanged)
                persisted = PersistCachedConfig();

            return (cacheChanged && persisted) || (!cacheChanged);
        }

        public IEnumerable<SubscriptionDto> GetAllByEndpointUrl(string endpointUrl)
        {
            WaitForInitializationToComplete();

            var subscriptionList = new List<SubscriptionDto>();
            foreach (var key in _managedSubscriptions.Keys)
            {
                if (key.EndpointUrl != endpointUrl)
                    continue;

                _managedSubscriptions.TryGetValue(key, out var value);

                subscriptionList.Add(value);
            }

            return subscriptionList;
        }

        public IEnumerable<SubscriptionDto> GetAllSubscriptions()
        {
            WaitForInitializationToComplete();

            var allSubscriptions = _managedSubscriptions.Values.ToList();
            return allSubscriptions;
        }

        public bool Remove(SubscriptionDto subscription)
        {
            WaitForInitializationToComplete();

            var persisted = false;
            var key = (subscription.EndpointUrl, subscription.PublishingInterval);
            var hasRemoved = _managedSubscriptions.Remove(key);

            if (hasRemoved)
                persisted = PersistCachedConfig();

            return (hasRemoved && persisted);
        }

        private void WaitForInitializationToComplete()
        {
            while (!_repositoryInitialized)
            { } //TODO: Is this necessarry and do it better if it is
        }
    }
}