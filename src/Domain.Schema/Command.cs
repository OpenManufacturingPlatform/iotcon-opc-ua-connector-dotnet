using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Enums;

namespace OMP.Connector.Domain.Schema
{
    public abstract class Command
    {
        [JsonProperty("commandType", Required = Required.Always)]
        [Description("Type of command request")]
        public OpcUaCommandType OpcUaCommandType { get; set; }
    }
}