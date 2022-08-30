// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Models.Subscriptions
{
    public record RemoveSubscriptionsCommand
    {
        public string EndpointUrl { get; set; } = string.Empty;
        public List<string> NodeIds { get; set; } = new List<string>();
    }
}
