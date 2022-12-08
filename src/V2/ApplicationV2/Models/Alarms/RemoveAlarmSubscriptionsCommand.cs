// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUa.Models.Alarms
{
    public record RemoveAlarmSubscriptionsCommand(string EndpointUrl, List<string> NodeIds);
}
