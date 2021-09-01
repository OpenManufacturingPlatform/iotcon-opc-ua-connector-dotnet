using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OMP.Connector.Domain.Schema.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RelationDirection
    {
        [EnumMember(Value = "in")] In,
        [EnumMember(Value = "out")] Out
    }
}