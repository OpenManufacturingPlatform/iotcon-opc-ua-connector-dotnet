// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUa.Models.Subscriptions;
using OMP.PlantConnectivity.OpcUa.Sessions;

namespace OMP.PlantConnectivity.OpcUa.Services.Subscriptions
{

    public interface ISubscriptionCommandService
    {
        Task<CreateSubscriptionResponse> CreateSubscriptions(IOpcUaSession opcUaSession, CreateSubscriptionsCommand command, CancellationToken CancellationToken);
        Task<RemoveSubscriptionsResponse> RemoveSubscriptionsCommand(IOpcUaSession opcUaSession, RemoveSubscriptionsCommand command, CancellationToken cancellationToken);
        Task<RemoveAllSubscriptionsResponse> RemoveAllSubscriptions(IOpcUaSession opcUaSession, RemoveAllSubscriptionsCommand command, CancellationToken cancellationToken);
    }
}
