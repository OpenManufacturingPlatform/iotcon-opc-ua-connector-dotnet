using System.ComponentModel;
using Newtonsoft.Json;

namespace OMP.Connector.Domain.Schema
{
    public class OutputArgument : InputArgument
    {
        [JsonProperty("dataType", Required = Required.Always)]
        [Description("Data Type of output value")]
        public string DataType { get; set; }
    }
}