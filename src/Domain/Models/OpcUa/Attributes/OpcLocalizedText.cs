// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Newtonsoft.Json;

namespace OMP.Connector.Domain.Models.OpcUa.Attributes
{
    public class OpcLocalizedText
    {
        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }
    }
}
