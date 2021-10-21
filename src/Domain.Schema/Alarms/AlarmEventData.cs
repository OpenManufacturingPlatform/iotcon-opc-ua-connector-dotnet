using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace OMP.Connector.Domain.Schema.Alarms
{
    public class AlarmEventData
    {
        [JsonProperty("eventProperties", Required = Required.Always)]
        [Description("Event properties")]
        public Dictionary<string, object> EventProperties { get; set; }
    }
}