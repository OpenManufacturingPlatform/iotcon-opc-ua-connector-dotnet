// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using Newtonsoft.Json;

namespace OMP.Connector.Domain.Models.OpcUa.Attributes
{
    public class OpcDataValue
    {
        [JsonProperty("value")]
        public object Value { get; set; }

        [JsonProperty("statusCode")]
        public string StatusCode { get; set; }

        [JsonProperty("sourceTimestamp")]
        public DateTime SourceTimestamp { get; set; }

        [JsonProperty("sourcePicoseconds")]
        public ushort SourcePicoseconds { get; set; }

        [JsonProperty("serverTimestamp")]
        public DateTime ServerTimestamp { get; set; }

        [JsonProperty("serverPicoseconds")]
        public ushort ServerPicoseconds { get; set; }
    }
}