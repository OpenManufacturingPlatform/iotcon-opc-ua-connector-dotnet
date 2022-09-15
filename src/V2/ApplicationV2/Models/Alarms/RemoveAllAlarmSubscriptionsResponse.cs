// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUA.Models.Alarms
{
    public record RemoveAllAlarmSubscriptionsResponse : CommandResultBase
    {
        public RemoveAllAlarmSubscriptionsCommand? Command { get; set; } = default;
    }
}
