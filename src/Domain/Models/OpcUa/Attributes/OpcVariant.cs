// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Newtonsoft.Json;

namespace OMP.Connector.Domain.Models.OpcUa.Attributes
{
    public class OpcVariant
    {
        [JsonProperty("value")]
        public object Value { get; set; }

        [JsonProperty("typeInfo")]
        public OpcTypeInfo TypeInfo { get; set; }
    }
}
