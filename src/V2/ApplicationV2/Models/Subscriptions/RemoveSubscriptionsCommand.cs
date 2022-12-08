// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUa.Models.Subscriptions
{
    public record RemoveSubscriptionsCommand(string EndpointUrl, List<string> NodeIds);
}
