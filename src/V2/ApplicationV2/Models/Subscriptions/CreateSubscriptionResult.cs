// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Opc.Ua;

namespace ApplicationV2.Models.Subscriptions
{
    public record CreateSubscriptionResult(IEnumerable<string> NodeId, IEnumerable<StatusCode> Statuscode);
    public record CreateSubscriptionResponse: CommandResult<CreateSubscriptionsCommand, CreateSubscriptionResult>;

}
