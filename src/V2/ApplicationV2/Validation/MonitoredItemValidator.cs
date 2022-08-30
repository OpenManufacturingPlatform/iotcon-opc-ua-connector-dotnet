// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using ApplicationV2.Models.Subscriptions;
using FluentValidation;

namespace ApplicationV2.Validation
{
    public class MonitoredItemValidator : AbstractValidator<SubscriptionMonitoredItem>
    {
        public MonitoredItemValidator()
        {

            RuleFor(p => p.SamplingInterval)
                .GreaterThan(0)
                .WithMessage($"{nameof(SubscriptionMonitoredItem.SamplingInterval)} has to be a positive number.");

            RuleFor(p => p.SamplingInterval)
                .Must((item, samplingInterval) => item.PublishingInterval >= samplingInterval)
                .WithMessage(
                    $"{nameof(SubscriptionMonitoredItem.PublishingInterval)} must be greater than {nameof(SubscriptionMonitoredItem.SamplingInterval)}");


            RuleFor(p => p.HeartbeatInterval)
            .Must((item, heartbeatInterval) =>  heartbeatInterval >= item.PublishingInterval)
            .WithMessage(
                    $"{nameof(SubscriptionMonitoredItem.HeartbeatInterval)} must be greater than {nameof(SubscriptionMonitoredItem.PublishingInterval)}");
        }
    }
}
