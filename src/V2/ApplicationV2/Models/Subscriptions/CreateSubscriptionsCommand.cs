// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUA.Models.Subscriptions
{
    public record CreateSubscriptionsCommand
    {
        public string EndpointUrl { get; }

        public List<SubscriptionMonitoredItem> MonitoredItems { get; set; } = new List<SubscriptionMonitoredItem>();

        public CreateSubscriptionsCommand(string endpointUrl)
        {
            EndpointUrl = endpointUrl;
        }

        public CreateSubscriptionsCommand(string endpointUrl, List<SubscriptionMonitoredItem> monitoredItems)
        {
            EndpointUrl = endpointUrl;
            MonitoredItems = monitoredItems;
        }
    }
}
