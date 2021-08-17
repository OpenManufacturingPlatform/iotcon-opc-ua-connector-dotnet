using System.ComponentModel;
using Newtonsoft.Json;

namespace Omp.Connector.Domain.Schema
{
    public class InputArgument
    {
        [JsonProperty("key", Required = Required.Always)]
        [Description("Argument key")]
        public string Key { get; set; }

        [JsonProperty("value", Required = Required.Always)]
        [Description("Argument value")]
        public string Value { get; set; }
    }
}