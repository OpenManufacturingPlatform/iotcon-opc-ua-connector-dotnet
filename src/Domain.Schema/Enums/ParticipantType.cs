using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Omp.Connector.Domain.Schema.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ParticipantType
    {
        [EnumMember(Value = "device")] Device,
        [EnumMember(Value = "gateway")] Gateway,
        [EnumMember(Value = "application")] Application
    }
}