// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Newtonsoft.Json;
using OMP.Connector.Domain.Models.OpcUa.Nodes.Base;

namespace OMP.Connector.Domain.Models.OpcUa.Nodes
{
    public class OpcView : OpcNode
    {
        [JsonProperty("containsNoLoops")]
        public bool ContainsNoLoops { get; set; }

        [JsonProperty("eventNotifier")]
        public byte EventNotifier { get; set; }
    }
}