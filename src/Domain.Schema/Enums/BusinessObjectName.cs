using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OMP.Connector.Domain.Schema.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BusinessObjectName
    {
        [EnumMember(Value = "akz")]
        Akz,
        [EnumMember(Value = "application")]
        Application,
        [EnumMember(Value = "applicationCategory")]
        ApplicationCategory,
        [EnumMember(Value = "applicationInstance")]
        ApplicationInstance,
        [EnumMember(Value = "applicationState")]
        ApplicationState,
        [EnumMember(Value = "applicationVariableGroup")]
        ApplicationVariableGroup,
        [EnumMember(Value = "applicationVariableGroupTemplate")]
        ApplicationVariableGroupTemplate,
        [EnumMember(Value = "applicationVersion")]
        ApplicationVersion,
        [EnumMember(Value = "capability")]
        Capability,
        [EnumMember(Value = "device")]
        Device,
        [EnumMember(Value = "deviceState")]
        DeviceState,
        [EnumMember(Value = "deviceTwin")]
        DeviceTwin,
        [EnumMember(Value = "deviceType")]
        DeviceType,
        [EnumMember(Value = "deviceVariableGroup")]
        DeviceVariableGroup,
        [EnumMember(Value = "deviceVariableGroupTemplate")]
        DeviceVariableGroupTemplate,
        [EnumMember(Value = "facility")]
        Facility,
        [EnumMember(Value = "lineSection")]
        LineSection,
        [EnumMember(Value = "networkSettings")]
        NetworkSettings,
        [EnumMember(Value = "organization")]
        Organization,
        [EnumMember(Value = "sharedDeviceVariableGroup")]
        SharedDeviceVariableGroup,
        [EnumMember(Value = "tenant")]
        Tenant,
        [EnumMember(Value = "user")]
        User,
        [EnumMember(Value = "userGroup")]
        UserGroup,
    }
}