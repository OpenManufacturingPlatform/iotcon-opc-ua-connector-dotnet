// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUA.Sessions.Subscriptions
{
    public interface ISubscriptionRestoreService
    {
        Task RestoreSubscriptionsAsync(IOpcUaSession opcUaSession);
    }
}
