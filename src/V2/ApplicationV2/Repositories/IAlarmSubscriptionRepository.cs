// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUa.Models.Alarms;

namespace OMP.PlantConnectivity.OpcUa.Repositories
{
    public interface IAlarmSubscriptionRepository
    {
        void Add(string endpointUrl, IEnumerable<AlarmSubscriptionMonitoredItem> monitoredItems);

        IEnumerable<AlarmSubscriptionDto> GetAllByEndpointUrl(string endpointUrl);

        void DeleteAlarmMonitoredItems(string endpointUrl, IEnumerable<string> nodeIds);

        bool Remove(AlarmSubscriptionDto subscription);
    }
}
