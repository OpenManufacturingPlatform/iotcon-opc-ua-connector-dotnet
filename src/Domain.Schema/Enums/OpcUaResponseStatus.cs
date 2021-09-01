using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OMP.Connector.Domain.Schema.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OpcUaResponseStatus
    {
        [EnumMember(Value = "Good")] Good,
        [EnumMember(Value = "Bad")] Bad
    }
}