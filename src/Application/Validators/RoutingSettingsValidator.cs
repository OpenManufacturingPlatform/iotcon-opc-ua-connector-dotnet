// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using FluentValidation;
using OMP.Connector.Domain.Configuration;

namespace OMP.Connector.Application.Validators
{
    public class RoutingSettingsValidator : AbstractValidator<ConnectorConfiguration>
    {
        public RoutingSettingsValidator()
        {
            //this.When(settings => !string.IsNullOrWhiteSpace(settings.Communication.KafkaBootstrapServers), () =>
            //{
            //    this.RuleFor(s => s.KafkaTopicComConDown)
            //        .NotEmpty()
            //        .WithMessage(s => $"{nameof(s.KafkaTopicComConDown)} is missing");

            //    this.RuleFor(s => s.KafkaTopicComConUp)
            //        .NotEmpty()
            //        .WithMessage(s => $"{nameof(s.KafkaTopicComConUp)} is missing");

            //    this.RuleFor(settings => settings.KafkaTopicTelemetry)
            //        .NotEmpty()
            //        .WithMessage(s => $"{nameof(s.KafkaTopicTelemetry)} is missing");

            //    this.RuleFor(settings => settings.KafkaConsumerGroup)
            //        .NotEmpty()
            //        .WithMessage(s => $"{nameof(s.KafkaConsumerGroup)} is missing");
            //});
        }
    }
}