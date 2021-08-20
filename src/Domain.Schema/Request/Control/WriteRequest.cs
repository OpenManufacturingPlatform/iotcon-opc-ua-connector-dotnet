using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Converters;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.Request.Control.Base;

namespace OMP.Connector.Domain.Schema.Request.Control
{
    public class WriteRequest : NodeCommandRequest
    {
        [JsonConverter(typeof(WriteRequestValueConverter))]
        [JsonProperty("value", Required = Required.Always)]
        [Description("Value to write")]
        public IWriteRequestValue Value { get; set; }
    }
}