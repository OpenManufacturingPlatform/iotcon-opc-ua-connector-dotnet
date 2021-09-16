using System.Linq;
using OMP.Connector.Domain.Schema;
using OMP.Connector.Domain.Schema.Enums;
using OMP.Connector.Domain.Schema.Messages;

namespace OMP.Connector.Domain.Models.Telemetry
{
    public class TelemetryMessageMetadata
    {
        public static TelemetryMessageMetadata MapFrom(CommandRequest commandRequest)
        {
            var instance = new TelemetryMessageMetadata()
            {
                Sender = commandRequest.MetaData?.DestinationIdentifiers?.FirstOrDefault(),
                DataSourceUrl = commandRequest.Payload.RequestTarget.EndpointUrl
            };

            if (instance.Sender == null)
                return instance;

            instance.Sender.Type = ParticipantType.Gateway;
            instance.Sender.Name = Constants.TelemetrySenderName;
            instance.Sender.Route = string.Empty;
            return instance;
        }

        public Participant Sender { get; set; }

        public string DataSourceUrl { get; set; }
    }
}