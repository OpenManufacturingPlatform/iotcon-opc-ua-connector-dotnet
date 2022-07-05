// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OMP.Connector.Domain.Schema.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OpcUaCommandType
    {
        [EnumMember(Value = "read")] Read,
        [EnumMember(Value = "write")] Write,
        [EnumMember(Value = "call")] Call,
        [EnumMember(Value = "browse")] Browse,

        [EnumMember(Value = "createSubscriptions")]
        CreateSubscription,

        [EnumMember(Value = "removeSubscriptions")]
        RemoveSubscriptions,

        [EnumMember(Value = "removeAllSubscriptions")]
        RemoveAllSubscriptions,

        [EnumMember(Value = "restoreSubscriptions")]
        RestoreSubscriptions,

        [EnumMember(Value = "createAlarmSubscriptions")]
        CreateAlarmSubscription,

        [EnumMember(Value = "removeAlarmSubscriptions")]
        RemoveAlarmSubscriptions,

        [EnumMember(Value = "removeAllAlarmSubscriptions")]
        RemoveAllAlarmSubscriptions,

        [EnumMember(Value = "restoreAlarmSubscriptions")]
        RestoreAlarmSubscriptions,

        [EnumMember(Value = "respondToAlarmEvents")]
        RespondToAlarmEvents,

        [EnumMember(Value = "serverDiscovery")]
        ServerDiscovery,

        [EnumMember(Value = "browseChildNodes")]
        BrowseChildNodes,

        [EnumMember(Value = "browseChildNodesFromRoot")]
        BrowseChildNodesFromRoot
    }
}