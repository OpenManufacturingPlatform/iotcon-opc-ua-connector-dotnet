// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Request.Discovery.Base;

namespace OMP.Connector.Domain.Schema.Request.Discovery
{
    public class ServerDiscoveryRequest : DiscoveryRequest
    {
        [JsonProperty("serverDetails", Required = Required.Always)]
        [Description("Details of server to discover")]
        public ServerDetails ServerDetails { get; set; }
    }
}