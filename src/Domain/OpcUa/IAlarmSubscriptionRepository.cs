// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Generic;
using OMP.Connector.Domain.Models;
using OMP.Connector.Domain.Schema;

namespace OMP.Connector.Domain.OpcUa
{
    public interface IAlarmSubscriptionRepository
    {
        IEnumerable<AlarmSubscriptionDto> GetAllSubscriptions();

        bool Add(AlarmSubscriptionDto subscription);

        bool Remove(AlarmSubscriptionDto subscription);

        IEnumerable<AlarmSubscriptionDto> GetAllByEndpointUrl(string endpointUrl);

        bool DeleteMonitoredItems(string endpointUrl, IEnumerable<OpcUaMonitoredItem> items);
    }
}
