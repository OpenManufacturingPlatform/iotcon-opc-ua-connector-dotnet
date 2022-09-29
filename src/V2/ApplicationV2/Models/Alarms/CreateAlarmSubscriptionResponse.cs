// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUA.Models.Alarms
{
    public record CreateAlarmSubscriptionResponse : CommandResult<CreateAlarmSubscriptionsCommand, CreateAlarmSubscriptionResult>
    {
        public CreateAlarmSubscriptionResponse(CreateAlarmSubscriptionsCommand command, CreateAlarmSubscriptionResult result, bool succeeded, string? errorMessage = null)
            : base(command, result, succeeded)
        {
            Message = errorMessage;
        }
    }
}
