// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace OMP.PlantConnectivity.OpcUA.Models.Alarms
{
    public record AlarmMonitoredItemResult(AlarmSubscriptionMonitoredItem AlarmMonitoredItem, StatusCode StatusCode, string Message);
}
