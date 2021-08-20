using System.Linq;
using FluentValidation;
using Microsoft.Extensions.Options;
using OMP.Connector.Domain.Configuration;
using Omp.Connector.Domain.Schema;
using Omp.Connector.Domain.Schema.Messages;

namespace OMP.Connector.Application.Validators
{
    public class CommandRequestValidator : AbstractValidator<CommandRequest>
    {
        public CommandRequestValidator(IOptions<ConnectorConfiguration> connectorConfiguration)
        {
            this.RuleFor(request => request.MetaData)
                .NotNull()
                .WithMessage($"{nameof(CommandRequest.MetaData)} is missing");

            this.When(request => request?.MetaData != null, () =>
            {
                this.RuleFor(request => request.MetaData.DestinationIdentifiers)
                    .NotNull()
                    .WithMessage($"{nameof(CommandRequest.MetaData.DestinationIdentifiers)} is missing");
            });

            this.When(request => request?.MetaData?.DestinationIdentifiers != null, () =>
            {
                this.RuleFor(request => request.MetaData.DestinationIdentifiers)
                    .NotEmpty()
                    .WithMessage($"{nameof(CommandRequest.MetaData.DestinationIdentifiers)} has to contain at least one {nameof(Participant)}");
            });

            this.When(request => request?.MetaData?.DestinationIdentifiers != null && request.MetaData.DestinationIdentifiers.Any(), () =>
            {
                this.RuleFor(request => request.MetaData.DestinationIdentifiers)
                    .Must(list => list.Any(a => !string.IsNullOrWhiteSpace(a.Id) && a.Id.Equals(connectorConfiguration.Value.ConnectorId)))
                    .WithMessage($"{nameof(Participant.Id)} is invalid in {nameof(CommandRequest.MetaData.DestinationIdentifiers)}");
            });
        }
    }
}