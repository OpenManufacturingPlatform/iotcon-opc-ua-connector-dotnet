// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Responses.Discovery.Base;

namespace OMP.Connector.Domain.Schema.Responses.Discovery
{
    public class BrowseChildNodesResponse : DiscoveryResponse
    {
        [JsonProperty("node", Required = Required.Always)]
        [Description("Start node for discovery")]
        public DiscoveredOpcNode DiscoveredOpcNode { get; set; }
    }
}