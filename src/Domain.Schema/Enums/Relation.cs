using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OMP.Connector.Domain.Schema.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Relation
    {
        [EnumMember(Value = "belongsToOrganization")]
        BelongsToOrganization,

        [EnumMember(Value = "belongsToTenant")]
        BelongsToTenant,

        [EnumMember(Value = "isAssignedToOrganization")]
        IsAssignedToOrganization,

        [EnumMember(Value = "isAssignedToTenant")]
        IsAssignedToTenant,
        [EnumMember(Value = "isCapableOf")] IsCapableOf,

        [EnumMember(Value = "isCategorizedBy")]
        IsCategorizedBy,

        [EnumMember(Value = "isCompatibleWith")]
        IsCompatibleWith,
        [EnumMember(Value = "isConfiguredBy")] IsConfiguredBy,

        [EnumMember(Value = "isConnectingToNetworkThrough")]
        IsConnectingToNetworkThrough,
        [EnumMember(Value = "isDerivedFrom")] IsDerivedFrom,
        [EnumMember(Value = "isGatewayOf")] IsGatewayOf,
        [EnumMember(Value = "isInstalledOn")] IsInstalledOn,
        [EnumMember(Value = "isInstanceOf")] IsInstanceOf,
        [EnumMember(Value = "isInState")] IsInState,
        [EnumMember(Value = "isLocatedAt")] IsLocatedAt,
        [EnumMember(Value = "isOrganizedBy")] IsOrganizedBy,

        [EnumMember(Value = "isRepresentedBy")]
        IsRepresentedBy,
        [EnumMember(Value = "isTypeOf")] IsTypeOf,
        [EnumMember(Value = "isUserOf")] IsUserOf
    }
}