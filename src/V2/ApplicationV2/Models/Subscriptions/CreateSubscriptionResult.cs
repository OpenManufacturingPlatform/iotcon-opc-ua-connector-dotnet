// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUA.Models.Subscriptions
{
    public class CreateSubscriptionResult : List<MonitoredItemResult>
    {
        public CreateSubscriptionResult(List<MonitoredItemResult> monitoredItems)
        {
            base.AddRange(monitoredItems);
        }
    }
}
