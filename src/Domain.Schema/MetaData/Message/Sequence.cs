using System.ComponentModel;
using Newtonsoft.Json;

namespace Omp.Connector.Domain.Schema.MetaData.Message
{
    public class Sequence
    {
        [JsonProperty("id")]
        [Description("An ID for your sequence")]
        public string Id { get; set; }

        [JsonProperty("count")]
        [Description("Count within sequence id")]
        public string Count { get; set; }
    }
}