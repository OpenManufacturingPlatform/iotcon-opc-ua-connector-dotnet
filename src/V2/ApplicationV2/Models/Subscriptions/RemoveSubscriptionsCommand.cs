// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUA.Models.Subscriptions
{
    public record RemoveSubscriptionsCommand(string EndpointUrl, List<string> NodeIds);
}
