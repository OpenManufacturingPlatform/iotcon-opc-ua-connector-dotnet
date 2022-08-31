// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Models.Subscriptions
{
    public record RemoveSubscriptionsCommand(string EndpointUrl, List<string> NodeIds);
}
