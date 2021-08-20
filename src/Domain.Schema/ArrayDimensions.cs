using System.Collections.Generic;
using Newtonsoft.Json;

namespace OMP.Connector.Domain.Schema
{
    public class ArrayDimensions
    {
        [JsonProperty("dataType", NullValueHandling = NullValueHandling.Ignore)]
        public string DataType { get; set; }

        [JsonProperty("values", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<object> Values { get; set; }
    }
}