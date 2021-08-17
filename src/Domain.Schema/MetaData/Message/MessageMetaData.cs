using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Omp.Connector.Domain.Schema.Attributes.Examples;
using Omp.Connector.Domain.Schema.Interfaces;

namespace Omp.Connector.Domain.Schema.MetaData.Message
{
    public class MessageMetaData : IMetaData
    {
        [Timestamp]
        [TimeStampExamples]
        [JsonProperty("timestamp", Required = Required.Always)]
        [Description("Timestamp in ISO8601 format. UTC should be used whenever possible")]
        public DateTime TimeStamp { get; set; }

        [JsonProperty("correlationIds", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string> CorrelationIds { get; set; }

        [JsonProperty("senderIdentifier", NullValueHandling = NullValueHandling.Ignore)]
        [Description("The Sender System")]
        public Participant SenderIdentifier { get; set; }

        [JsonProperty("destinationIdentifiers", NullValueHandling = NullValueHandling.Ignore)]
        [Description("List of Destination Systems")]
        public IEnumerable<Participant> DestinationIdentifiers { get; set; }

        [JsonProperty("sequence", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Sequence a sender can specify to group messages or count up to sort")]
        public Sequence Sequence { get; set; }
    }
}