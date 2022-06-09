﻿// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using FluentValidation;
using OMP.Connector.Domain.Schema;

namespace OMP.Connector.Application.Validators
{
    public class AlarmMonitoredItemValidator : AbstractValidator<AlarmSubscriptionMonitoredItem>
    {
        public AlarmMonitoredItemValidator()
        {

            this.Transform(item => item.SamplingInterval, value => int.TryParse(value, out int val) ? (int?)val : null)
                .GreaterThan(0)
                .WithMessage($"{nameof(AlarmSubscriptionMonitoredItem.SamplingInterval)} has to be a positive number.");

            this.Transform(item => new Tuple<string, string>(item.SamplingInterval, item.PublishingInterval).ToValueTuple(), tuple => (
                     int.TryParse(tuple.Item1, out int val1) ? (int?)val1 : null,
                     int.TryParse(tuple.Item2, out int val2) ? (int?)val2 : null)
                )
                .Must(tuple => tuple.Item1 != null && tuple.Item2 != null && tuple.Item1 <= tuple.Item2)
                .WithMessage(
                    $"{nameof(AlarmSubscriptionMonitoredItem.PublishingInterval)} must be greater than {nameof(AlarmSubscriptionMonitoredItem.SamplingInterval)}");


            this.When(item => item.HeartbeatInterval != null, () =>
            {
                this.Transform(item => new Tuple<string, string>(item.PublishingInterval, item.HeartbeatInterval).ToValueTuple(),
                    tuple => (
                        int.TryParse(tuple.Item1, out int val1) ? (int?)val1 : null,
                        int.TryParse(tuple.Item2, out int val2) ? (int?)val2 : null)
                    )
                    .Must(tuple => tuple.Item1 != null && tuple.Item2 != null && tuple.Item1 <= tuple.Item2)
                    .WithMessage(
                        $"{nameof(AlarmSubscriptionMonitoredItem.HeartbeatInterval)} must be greater than {nameof(AlarmSubscriptionMonitoredItem.PublishingInterval)}");
            });
        }
    }
}
