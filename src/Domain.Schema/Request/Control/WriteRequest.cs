using System.ComponentModel;
using Newtonsoft.Json;
using Omp.Connector.Domain.Schema.Converters;
using Omp.Connector.Domain.Schema.Interfaces;
using Omp.Connector.Domain.Schema.Request.Control.Base;

namespace Omp.Connector.Domain.Schema.Request.Control
{
    public class WriteRequest : NodeCommandRequest
    {
        [JsonConverter(typeof(WriteRequestValueConverter))]
        [JsonProperty("value", Required = Required.Always)]
        [Description("Value to write")]
        public IWriteRequestValue Value { get; set; }
    }
}