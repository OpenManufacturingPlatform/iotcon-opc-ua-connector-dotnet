// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUa.Sessions.Subscriptions
{
    public interface ISubscriptionRestoreService
    {
        Task RestoreSubscriptionsAsync(IOpcUaSession opcUaSession);
    }
}
