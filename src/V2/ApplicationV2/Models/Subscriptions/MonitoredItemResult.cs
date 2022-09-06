// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUA.Models.Subscriptions
{
    public record MonitoredItemResult(SubscriptionMonitoredItem MonitoredItem, StatusCode StatusCode, string Message);

}
