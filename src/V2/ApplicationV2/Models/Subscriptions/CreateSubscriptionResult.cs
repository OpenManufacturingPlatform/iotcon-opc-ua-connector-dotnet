// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Models.Subscriptions
{
    public class CreateSubscriptionResult : List<MonitoredItemResult>
    {
        public CreateSubscriptionResult(List<MonitoredItemResult> monitoredItems)
        {
            base.AddRange(monitoredItems);
        }
    }
}
