using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema;
using OMP.Connector.Domain.Schema.Attributes.Examples;
using OMP.Connector.Domain.Schema.Attributes.Regex;

namespace OMP.Connector.Domain.Models
{
    public class AlarmSubscriptionDto
    {
        [Guid]
        [GuidExamples]
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("endpointUrl", Required = Required.Always)]
        [Description("endpointUrl")]
        public string EndpointUrl { get; set; }

        [JsonProperty("publishingInterval", Required = Required.Always)]
        [Description("publishingInterval")]
        public string PublishingInterval { get; set; }

        [JsonProperty("monitoredItems", Required = Required.Always)]
        [Description("monitoredItems")]
        public IDictionary<string, AlarmSubscriptionMonitoredItem> MonitoredItems { get; set; }
    }
}