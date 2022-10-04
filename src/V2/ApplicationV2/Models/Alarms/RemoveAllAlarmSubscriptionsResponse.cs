// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUa.Models.Alarms
{
    public record RemoveAllAlarmSubscriptionsResponse : CommandResultBase
    {
        public RemoveAllAlarmSubscriptionsCommand? Command { get; set; } = default;
    }
}
