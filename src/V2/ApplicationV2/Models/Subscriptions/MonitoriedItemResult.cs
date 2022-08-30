// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace ApplicationV2.Models.Subscriptions
{
    public record MonitoriedItemResult(SubscriptionMonitoredItem MonitoredItem, StatusCode StatusCode, string Message);

}
