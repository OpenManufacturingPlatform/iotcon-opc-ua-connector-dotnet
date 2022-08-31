// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Models.Subscriptions
{
    public class CreateSubscriptionResult : List<MonitoriedItemResult>
    {
        public CreateSubscriptionResult(List<MonitoriedItemResult> monitoriedItems)
        {
            base.AddRange(monitoriedItems);
        }
    }
}
