// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel;
using Newtonsoft.Json;

namespace OMP.Connector.Domain.Schema.Responses.Subscription
{
    public class CreateSubscriptionItemResponse : OpcUaMonitoredItem
    {
        [JsonProperty("errorSource", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Source component where the error response was sent from")]
        public string ErrorSource { get; set; }

        [JsonProperty("message", Required = Required.Always)]
        [Description("Human readable result of the request execution")]
        public string Message { get; set; }

        [JsonProperty("statusCode", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Code related to the errorSource describing the reason for the error response")]
        public string StatusCode { get; set; }
    }
}