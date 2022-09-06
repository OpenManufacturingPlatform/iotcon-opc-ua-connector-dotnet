// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUA.Models.Subscriptions;

namespace OMP.PlantConnectivity.OpcUA.Repositories
{
    public interface ISubscriptionRepository
    {
        void Add(string endpointUrl, IEnumerable<SubscriptionMonitoredItem> monitoredItems);

        IEnumerable<SubscriptionDto> GetAllByEndpointUrl(string endpointUrl);

        void DeleteMonitoredItems(string endpointUrl, IEnumerable<string> nodeIds);

        bool Remove(SubscriptionDto subscription);
    }
}
