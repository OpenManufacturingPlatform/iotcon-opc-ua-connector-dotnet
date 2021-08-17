using System.ComponentModel;
using Newtonsoft.Json;
using Omp.Connector.Domain.Schema.Converters;
using Omp.Connector.Domain.Schema.Interfaces;

namespace Omp.Connector.Domain.Schema.Request.Control.WriteValues
{
    public class WriteRequestValue : IWriteRequestValue
    {
        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Key for value")]
        public string Key { get; set; }


        [JsonProperty("dataType", NullValueHandling = NullValueHandling.Ignore)]
        [Description("The route of the node")]
        public string DataType { get; set; }
        
        [JsonConverter(typeof(WriteRequestValueConverter))]
        [JsonProperty("value", Required = Required.AllowNull)]
        public IWriteRequestValue Value { get; set; }
    }
}