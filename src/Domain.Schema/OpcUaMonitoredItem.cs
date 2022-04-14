// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Newtonsoft.Json;

namespace OMP.Connector.Domain.Schema
{
    public class OpcUaMonitoredItem
    {
        [JsonProperty("nodeId", Required = Required.Always)]
        public string NodeId { get; set; }
    }
}