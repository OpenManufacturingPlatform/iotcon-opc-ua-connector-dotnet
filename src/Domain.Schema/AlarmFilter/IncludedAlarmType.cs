// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel;
using Newtonsoft.Json;

namespace OMP.Connector.Domain.Schema.AlarmFilter
{
    public class IncludedAlarmType
    {
        [JsonProperty("typeNodeId", Required = Required.Always)]
        [Description("Node id of alarm type to include")]
        public string TypeNodeId { get; set; }

        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Fields to include for this alarm type - if null or empty, all fields of this type included by default")]
        public string[] Fields { get; set; }
    }
}
