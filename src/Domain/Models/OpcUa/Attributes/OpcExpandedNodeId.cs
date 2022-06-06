// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Newtonsoft.Json;
using Opc.Ua;

namespace OMP.Connector.Domain.Models.OpcUa.Attributes
{
    public class OpcExpandedNodeId
    {
        [JsonProperty("namespaceIndex")]
        public ushort NamespaceIndex { get; set; }

        [JsonProperty("idType")]
        public IdType IdType { get; set; }

        [JsonProperty("identifier")]
        public object Identifier { get; set; }

        [JsonProperty("isAbsolute")]
        public bool IsAbsolute { get; set; }

        [JsonProperty("isNull")]
        public bool IsNull { get; set; }

        [JsonProperty("namespaceUri")]
        public string NamespaceUri { get; set; }

        [JsonProperty("serverIndex")]
        public uint ServerIndex { get; set; }
    }
}