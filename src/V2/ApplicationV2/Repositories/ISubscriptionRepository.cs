// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using ApplicationV2.Models.Subscriptions;

namespace ApplicationV2.Repositories
{
    public interface ISubscriptionRepository
    {
        void Add(string endpointUrl, IEnumerable<SubscriptionMonitoredItem> monitoredItems);
    }
}
