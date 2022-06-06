// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Base;

namespace OMP.Connector.Domain.Schema.Alarms
{
    public class AlarmSource : Source
    {
        [JsonProperty("endpointUrl", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Endpoint URL of the source")]
        public string EndpointUrl { get; set; }
    }
}
