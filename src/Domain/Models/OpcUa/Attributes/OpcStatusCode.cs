// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Newtonsoft.Json;
using Opc.Ua;

namespace OMP.Connector.Domain.Models.OpcUa.Attributes
{
    public class OpcStatusCode
    {
        [JsonProperty("limitBits")]
        public LimitBits LimitBits { get; set; }

        [JsonProperty("hasDataValueInfo")]
        public bool HasDataValueInfo { get; set; }

        [JsonProperty("semanticsChanged")]
        public bool SemanticsChanged { get; set; }

        [JsonProperty("structureChanged")]
        public bool StructureChanged { get; set; }

        [JsonProperty("subCode")]
        public uint SubCode { get; set; }

        [JsonProperty("flagBits")]
        public uint FlagBits { get; }

        [JsonProperty("codeBits")]
        public uint CodeBits { get; }

        [JsonProperty("code")]
        public uint Code { get; set; }

        [JsonProperty("overflow")]
        public bool Overflow { get; set; }

        [JsonProperty("aggregateBits")]
        public AggregateBits AggregateBits { get; set; }
    }
}