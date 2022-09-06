// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

namespace OMP.PlantConnectivity.OpcUA.Models.Subscriptions
{
    public record CreateSubscriptionResponse : CommandResult<CreateSubscriptionsCommand, CreateSubscriptionResult>
    {
        public CreateSubscriptionResponse(CreateSubscriptionsCommand command, CreateSubscriptionResult result, bool succeeded, string? errorMessage = null)
            : base(command, result, succeeded)
        {
            this.Message = errorMessage;
        }
    }
}
