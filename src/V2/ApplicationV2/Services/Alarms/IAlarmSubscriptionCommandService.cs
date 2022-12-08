// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUa.Models.Alarms;
using OMP.PlantConnectivity.OpcUa.Sessions;
using CreateAlarmSubscriptionResponse = OMP.PlantConnectivity.OpcUa.Models.Alarms.CreateAlarmSubscriptionResponse;

namespace OMP.PlantConnectivity.OpcUa.Services.Alarms
{

    public interface IAlarmSubscriptionCommandService
    {
        Task<CreateAlarmSubscriptionResponse> CreateAlarmSubscriptions(IOpcUaSession opcUaSession, CreateAlarmSubscriptionsCommand command, CancellationToken CancellationToken);
        Task<RemoveAlarmSubscriptionsResponse> RemoveAlarmSubscriptionsCommand(IOpcUaSession opcUaSession, RemoveAlarmSubscriptionsCommand command, CancellationToken cancellationToken);
        Task<RemoveAllAlarmSubscriptionsResponse> RemoveAllAlarmSubscriptions(IOpcUaSession opcUaSession, RemoveAllAlarmSubscriptionsCommand command, CancellationToken cancellationToken);
    }
}
