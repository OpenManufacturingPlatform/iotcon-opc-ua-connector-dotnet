using System;
using Newtonsoft.Json;

namespace OMP.Connector.Domain.Models.OpcUa.Attributes
{
    public class OpcEventFieldList
    {
        [JsonProperty("eventFields")]
        public object[] EventFields { get; set; }
    }
}