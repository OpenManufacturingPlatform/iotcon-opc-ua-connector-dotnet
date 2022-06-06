// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel;
using Newtonsoft.Json;

namespace OMP.Connector.Domain.Schema
{
    public class ServerDetails
    {
        [JsonProperty("name", Required = Required.Always)]
        [Description("name")]
        public string Name { get; set; }

        [JsonProperty("route", Required = Required.Always)]
        [Description("Route")]
        public string Route { get; set; }
    }
}