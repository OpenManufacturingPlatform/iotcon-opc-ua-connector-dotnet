// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Models.Subscriptions
{
    public record SubscriptionMonitoredItem
    {
        public string NodeId { get; set; } = string.Empty;

        public string SamplingInterval { get; set; } = string.Empty;

        public string PublishingInterval { get; set; } = string.Empty;

        public string HeartbeatInterval { get; set; } = string.Empty;
    }


}
