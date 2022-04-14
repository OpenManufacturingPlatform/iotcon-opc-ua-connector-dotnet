// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel;
using Newtonsoft.Json;

namespace OMP.Connector.Domain.Schema.Request
{
    public class RequestTarget
    {
        [JsonProperty("endpointUrl", Required = Required.Always)]
        [Description("Base url of the OPC UA server")]
        public string EndpointUrl { get; set; }
    }
}