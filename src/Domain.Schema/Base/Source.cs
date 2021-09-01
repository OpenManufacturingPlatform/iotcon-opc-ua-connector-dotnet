using System.ComponentModel;
using Newtonsoft.Json;

namespace OMP.Connector.Domain.Schema.Base
{
    public abstract class Source
    {
        [Attributes.Regex.Guid]
        [Attributes.Examples.GuidExamples]
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        [Description("Name of data source, e.g. AKZ")]
        public string Name { get; set; }

        [JsonProperty("route")]
        [Description("Route on data source")]
        public string Route { get; set; }
    }
}