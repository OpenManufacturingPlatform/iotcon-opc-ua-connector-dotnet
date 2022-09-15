// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUA.Models.Alarms;

namespace OMP.PlantConnectivity.OpcUA.Repositories
{
    public interface IAlarmSubscriptionRepository
    {
        void Add(string endpointUrl, IEnumerable<AlarmSubscriptionMonitoredItem> monitoredItems);

        IEnumerable<AlarmSubscriptionDto> GetAllByEndpointUrl(string endpointUrl);

        void DeleteAlarmMonitoredItems(string endpointUrl, IEnumerable<string> nodeIds);

        bool Remove(AlarmSubscriptionDto subscription);
    }
}
