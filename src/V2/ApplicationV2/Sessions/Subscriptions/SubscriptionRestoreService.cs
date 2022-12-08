// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUa.Sessions.Subscriptions
{
    internal class FakeSubscriptionRestoreService : ISubscriptionRestoreService
    {
        public Task RestoreSubscriptionsAsync(IOpcUaSession opcUaSession)
        {
            throw new NotImplementedException();
        }
    }
}
