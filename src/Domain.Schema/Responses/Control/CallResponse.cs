// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Responses.Control.Base;

namespace OMP.Connector.Domain.Schema.Responses.Control
{
    public class CallResponse : NodeCommandResponse
    {
        [JsonProperty("arguments", Required = Required.Always)]
        [Description("Output arguments")]
        public IEnumerable<OutputArgument> Arguments { get; set; }
    }
}