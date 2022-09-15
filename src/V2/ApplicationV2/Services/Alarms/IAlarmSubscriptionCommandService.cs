// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUA.Models.Alarms;
using OMP.PlantConnectivity.OpcUA.Sessions;
using CreateAlarmSubscriptionResponse = OMP.PlantConnectivity.OpcUA.Models.Alarms.CreateAlarmSubscriptionResponse;

namespace OMP.PlantConnectivity.OpcUA.Services.Alarms
{

    public interface IAlarmSubscriptionCommandService
    {
        Task<CreateAlarmSubscriptionResponse> CreateAlarmSubscriptions(IOpcUaSession opcUaSession, CreateAlarmSubscriptionsCommand command, CancellationToken CancellationToken);
        Task<RemoveAlarmSubscriptionsResponse> RemoveAlarmSubscriptionsCommand(IOpcUaSession opcUaSession, RemoveAlarmSubscriptionsCommand command, CancellationToken cancellationToken);
        Task<RemoveAllAlarmSubscriptionsResponse> RemoveAllAlarmSubscriptions(IOpcUaSession opcUaSession, RemoveAllAlarmSubscriptionsCommand command, CancellationToken cancellationToken);
    }
}
