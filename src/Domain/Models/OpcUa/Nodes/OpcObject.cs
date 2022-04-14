// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Newtonsoft.Json;
using OMP.Connector.Domain.Models.OpcUa.Nodes.Base;

namespace OMP.Connector.Domain.Models.OpcUa.Nodes
{
    public class OpcObject : OpcNode
    {
        [JsonProperty("eventNotifier")]
        public byte EventNotifier { get; set; }
    }
}
