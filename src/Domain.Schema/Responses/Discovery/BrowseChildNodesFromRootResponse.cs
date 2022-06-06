// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Responses.Discovery.Base;

namespace OMP.Connector.Domain.Schema.Responses.Discovery
{
    public class BrowseChildNodesFromRootResponse : DiscoveryResponse
    {
        [JsonProperty("nodes", Required = Required.Always)]
        [Description("discovered root nodes")]
        public IEnumerable<DiscoveredOpcNode> DiscoveredOpcNodes { get; set; }
    }
}