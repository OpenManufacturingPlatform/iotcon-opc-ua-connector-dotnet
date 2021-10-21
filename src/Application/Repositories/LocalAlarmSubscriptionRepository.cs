using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using OMP.Connector.Domain.Models;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Schema;

namespace OMP.Connector.Application.Repositories
{
    public class LocalAlarmSubscriptionRepository : IAlarmSubscriptionRepository
    {
        private readonly IDictionary<Tuple<string, string>, AlarmSubscriptionDto> _managedSubscriptions;

        public LocalAlarmSubscriptionRepository()
        {
            this._managedSubscriptions = new ConcurrentDictionary<Tuple<string, string>, AlarmSubscriptionDto>();
        }

        public IEnumerable<AlarmSubscriptionDto> GetAllSubscriptions() => this._managedSubscriptions.Values.ToList();

        public bool Add(AlarmSubscriptionDto subscription)
        {
            var normalizedSubscriptions = this.GroupByPublishingInterval(subscription);
            foreach (var normalizedSubscription in normalizedSubscriptions)
            {
                this.UpdateSubscriptionCache(normalizedSubscription, true);
            }

            return true;
        }

        public bool Remove(AlarmSubscriptionDto subscription)
        {
            var key = new Tuple<string, string>(subscription.EndpointUrl, subscription.PublishingInterval);
            return this._managedSubscriptions.Remove(key);
        }

        public IEnumerable<AlarmSubscriptionDto> GetAllByEndpointUrl(string endpointUrl)
        {
            var subscriptionList = new List<AlarmSubscriptionDto>();
            foreach (var key in this._managedSubscriptions.Keys)
            {
                if (key.Item1 != endpointUrl)
                    continue;

                this._managedSubscriptions.TryGetValue(key, out var value);

                subscriptionList.Add(value);
            }

            return subscriptionList;
        }

        public bool DeleteMonitoredItems(string endpointUrl, IEnumerable<OpcUaMonitoredItem> items)
        {
            this.RemoveMonitoredItemsFromCache(endpointUrl, items);
            return true;
        }

        #region private methods

        private bool RemoveMonitoredItemsFromCache(string endpointUrl, IEnumerable<OpcUaMonitoredItem> items)
        {
            var itemsToDelete = items.ToList();

            var cacheChanged = false;
            foreach (var subscriptionKey in this._managedSubscriptions.Keys)
            {
                if (!this._managedSubscriptions.TryGetValue(subscriptionKey, out var subscriptionDto))
                    throw new Exception(
                        $"{nameof(LocalSubscriptionRepository)} internal cache inconsistent. subscriptionsKey: {subscriptionKey}");

                if (subscriptionDto.EndpointUrl != endpointUrl)
                    continue;

                var subscriptionItems = subscriptionDto.MonitoredItems;
                foreach (var nodeId in itemsToDelete.Select(x => x.NodeId))
                    cacheChanged = subscriptionItems.Remove(nodeId) || cacheChanged;

                if (subscriptionDto.MonitoredItems.Count == 0)
                {
                    this._managedSubscriptions.Remove(subscriptionKey);
                }
            }

            return cacheChanged;
        }

        private IEnumerable<AlarmSubscriptionDto> GroupByPublishingInterval(AlarmSubscriptionDto subscription)
        {
            if (subscription.PublishingInterval != null)
                return new List<AlarmSubscriptionDto> { subscription };

            var result = new List<AlarmSubscriptionDto>();
            var groups = subscription.MonitoredItems
                .GroupBy(x => x.Value.PublishingInterval);
            foreach (var group in groups)
            {
                var newMonitoredItemDict = group
                    .ToDictionary(x => x.Key, x => x.Value);
                var newSubscription = new AlarmSubscriptionDto
                {
                    EndpointUrl = subscription.EndpointUrl,
                    PublishingInterval = group.Key,
                    MonitoredItems = newMonitoredItemDict
                };
                result.Add(newSubscription);
            }

            return result;
        }

        private bool UpdateSubscriptionCache(AlarmSubscriptionDto candidatSubscription, bool overrideExistingItem)
        {
            if (overrideExistingItem)
                this.RemoveMonitoredItemsFromCache(candidatSubscription.EndpointUrl, candidatSubscription.MonitoredItems.Values);

            var key = new Tuple<string, string>(candidatSubscription.EndpointUrl, candidatSubscription.PublishingInterval);
            if (!this._managedSubscriptions.TryGetValue(key, out var cachedSubscription))
            {
                this._managedSubscriptions.Add(key, candidatSubscription);
                return true;
            }

            var candidatNodeIds = candidatSubscription.MonitoredItems.Keys;
            foreach (var candidatNodeId in candidatNodeIds)
            {
                candidatSubscription.MonitoredItems.TryGetValue(candidatNodeId, out var candidatMonitoredItem);

                if (!cachedSubscription.MonitoredItems.TryGetValue(candidatNodeId, out var cachedMonitoredItem))
                {
                    cachedSubscription.MonitoredItems.Add(candidatNodeId, candidatMonitoredItem);
                    return true;
                }

                if (overrideExistingItem && !cachedMonitoredItem.Equals(candidatMonitoredItem))
                {
                    cachedSubscription.MonitoredItems[candidatNodeId] = candidatMonitoredItem;
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}