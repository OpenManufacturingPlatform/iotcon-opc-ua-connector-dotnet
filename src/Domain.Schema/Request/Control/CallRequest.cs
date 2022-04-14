// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Request.Control.Base;

namespace OMP.Connector.Domain.Schema.Request.Control
{
    public class CallRequest : NodeCommandRequest
    {
        [JsonProperty("arguments", Required = Required.Always)]
        [Description("Input arguments")]
        public IEnumerable<InputArgument> Arguments { get; set; }
    }
}