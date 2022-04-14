// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel;
using Newtonsoft.Json;

namespace OMP.Connector.Domain.Schema
{
    public class InputArgument
    {
        [JsonProperty("key", Required = Required.Always)]
        [Description("Argument key")]
        public string Key { get; set; }

        [JsonProperty("value", Required = Required.Always)]
        [Description("Argument value")]
        public string Value { get; set; }
    }
}