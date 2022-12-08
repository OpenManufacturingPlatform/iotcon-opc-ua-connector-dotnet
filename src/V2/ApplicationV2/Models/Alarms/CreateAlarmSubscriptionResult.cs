// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUa.Models.Alarms
{
    public class CreateAlarmSubscriptionResult : List<AlarmMonitoredItemResult>
    {
        public CreateAlarmSubscriptionResult(List<AlarmMonitoredItemResult> alarmMonitoredItems)
        {
            AddRange(alarmMonitoredItems);
        }
    }
}
