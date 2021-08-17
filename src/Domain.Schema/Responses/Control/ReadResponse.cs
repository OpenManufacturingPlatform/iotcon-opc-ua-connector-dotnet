using System.ComponentModel;
using Newtonsoft.Json;
using Omp.Connector.Domain.Schema.Converters;
using Omp.Connector.Domain.Schema.Interfaces;
using Omp.Connector.Domain.Schema.Responses.Control.Base;

namespace Omp.Connector.Domain.Schema.Responses.Control
{
    public class ReadResponse : NodeCommandResponse
    {
        [JsonProperty("dataType", Required = Required.Always)]
        [Description("Data type of the value")]
        public string DataType { get; set; }

        [JsonConverter(typeof(SensorMeasurementConverter))]
        [JsonProperty("value", Required = Required.AllowNull)]
        [Description("Value of the node")]
        public IMeasurementValue Value { get; set; }
    }
}