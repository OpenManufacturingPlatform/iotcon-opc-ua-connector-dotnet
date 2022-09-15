// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUA.Models.Alarms
{
    public record CreateAlarmSubscriptionsCommand
    {
        public string EndpointUrl { get; }

        public List<AlarmSubscriptionMonitoredItem> AlarmMonitoredItems { get; set; } = new List<AlarmSubscriptionMonitoredItem>();

        public CreateAlarmSubscriptionsCommand(string endpointUrl)
        {
            EndpointUrl = endpointUrl;
        }

        public CreateAlarmSubscriptionsCommand(string endpointUrl, List<AlarmSubscriptionMonitoredItem> monitoredItems)
        {
            EndpointUrl = endpointUrl;
            AlarmMonitoredItems = monitoredItems;
        }
    }
}
