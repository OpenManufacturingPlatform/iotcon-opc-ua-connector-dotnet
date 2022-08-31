// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Models.Subscriptions
{
    public record RemoveAllSubscriptionsResponse : CommandResultBase
    {
        public RemoveAllSubscriptionsCommand? Command { get; set; } = default;
    }
}
