// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using FluentValidation;
using OMP.PlantConnectivity.OpcUA.Models.Alarms;

namespace OMP.Connector.Application.Validators
{
    public class AlarmMonitoredItemValidator : AbstractValidator<AlarmSubscriptionMonitoredItem>
    {
        public AlarmMonitoredItemValidator()
        {
            RuleFor(p => p.HeartbeatInterval)
            .Must((item, heartbeatInterval) => heartbeatInterval >= item.PublishingInterval)
            .WithMessage(
                    $"{nameof(AlarmSubscriptionMonitoredItem.HeartbeatInterval)} must be greater than {nameof(AlarmSubscriptionMonitoredItem.PublishingInterval)}");
        }
    }
}
