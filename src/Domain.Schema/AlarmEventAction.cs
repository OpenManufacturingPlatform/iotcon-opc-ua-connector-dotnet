using System.ComponentModel;
using Newtonsoft.Json;

namespace OMP.Connector.Domain.Schema
{
    public class AlarmEventAction
    {
        [JsonProperty("sourceNodeId", Required = Required.Always)]
        [Description("Id of source node where event was raised")]
        public string SourceNodeId { get; set; }

        [JsonProperty("eventId", Required = Required.Always)]
        [Description("Id of alarm event to act on")]
        public byte[] EventId { get; set; }

        [JsonProperty("comment", Required = Required.Always)]
        [Description("Comment for the event being act on")]
        public string Comment { get; set; }

        [JsonProperty("action", Required = Required.Always)]
        [Description("Action to perform on event")]
        public string Action { get; set; }
    }
}