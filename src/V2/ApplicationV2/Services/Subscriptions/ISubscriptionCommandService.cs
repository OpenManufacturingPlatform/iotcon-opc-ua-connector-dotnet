// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUA.Models.Subscriptions;
using OMP.PlantConnectivity.OpcUA.Sessions;

namespace OMP.PlantConnectivity.OpcUA.Services.Subscriptions
{

    public interface ISubscriptionCommandService
    {
        Task<CreateSubscriptionResponse> CreateSubscriptions(IOpcUaSession opcUaSession, CreateSubscriptionsCommand command, CancellationToken CancellationToken);
        Task<RemoveSubscriptionsResponse> RemoveSubscriptionsCommand(IOpcUaSession opcUaSession, RemoveSubscriptionsCommand command, CancellationToken cancellationToken);
        Task<RemoveAllSubscriptionsResponse> RemoveAllSubscriptions(IOpcUaSession opcUaSession, RemoveAllSubscriptionsCommand command, CancellationToken cancellationToken);
    }
}
