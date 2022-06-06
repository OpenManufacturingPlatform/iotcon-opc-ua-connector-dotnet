// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Newtonsoft.Json;

namespace OMP.Connector.Domain.Models.OpcUa.Attributes
{
    public class OpcQualifiedName
    {
        [JsonProperty("namespaceIndex")]
        public ushort NamespaceIndex { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}