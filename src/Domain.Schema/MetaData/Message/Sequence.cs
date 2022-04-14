// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel;
using Newtonsoft.Json;

namespace OMP.Connector.Domain.Schema.MetaData.Message
{
    public class Sequence
    {
        [JsonProperty("id")]
        [Description("An ID for your sequence")]
        public string Id { get; set; }

        [JsonProperty("count")]
        [Description("Count within sequence id")]
        public string Count { get; set; }
    }
}