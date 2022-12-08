// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUa.Models.Subscriptions
{
    public record RemoveAllSubscriptionsResponse : CommandResultBase
    {
        public RemoveAllSubscriptionsCommand? Command { get; set; } = default;
    }
}
