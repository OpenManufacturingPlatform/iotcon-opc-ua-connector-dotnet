using System.Collections.Generic;
using OMP.Connector.Domain.Models;
using OMP.Connector.Domain.Schema;

namespace OMP.Connector.Domain.OpcUa
{
    public interface ISubscriptionRepository
    {
        IEnumerable<SubscriptionDto> GetAllSubscriptions();

        bool Add(SubscriptionDto subscription);

        bool Remove(SubscriptionDto subscription);

        IEnumerable<SubscriptionDto> GetAllByEndpointUrl(string endpointUrl);

        bool DeleteMonitoredItems(string endpointUrl, IEnumerable<OpcUaMonitoredItem> items);
    }
}