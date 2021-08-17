using System.ComponentModel;
using Newtonsoft.Json;
using Omp.Connector.Domain.Schema.Enums;

namespace Omp.Connector.Domain.Schema
{
    public abstract class Command
    {
        [JsonProperty("commandType", Required = Required.Always)]
        [Description("Type of command request")]
        public OpcUaCommandType OpcUaCommandType { get; set; }
    }
}