// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace ApplicationV2.Sessions.Subscriptions
{
    public interface ISubscriptionRestoreService
    {
        Task RestoreSubscriptionsAsync(IOpcUaSession opcUaSession);
    }
}
