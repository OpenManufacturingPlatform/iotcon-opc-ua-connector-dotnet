// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Attributes.Examples;
using OMP.Connector.Domain.Schema.Attributes.Regex;
using OMP.Connector.Domain.Schema.Enums;

namespace OMP.Connector.Domain.Schema
{
    public class Participant
    {
        [Guid]
        [GuidExamples]
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        [Description("Name of sender or recipient")]
        public string Name { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        [Description("Type of sender or receiving system")]
        public ParticipantType Type { get; set; }

        [JsonProperty("route")]
        [Description("Route on sending or receiving system")]
        public string Route { get; set; }
    }
}