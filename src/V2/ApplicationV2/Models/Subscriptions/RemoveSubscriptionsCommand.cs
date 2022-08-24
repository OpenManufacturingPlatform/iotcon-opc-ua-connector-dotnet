// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Models.Subscriptions
{
    public record RemoveSubscriptionsCommand
    {
        public string[] NodeIds { get; set; } = new string[0];
    }


}
