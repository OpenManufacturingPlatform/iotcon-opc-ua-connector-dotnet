// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Request.AlarmSubscription.Base;

namespace OMP.Connector.Domain.Schema.Request.AlarmSubscription
{
    public class RespondToAlarmEventsRequest : AlarmSubscriptionRequest
    {
        [JsonProperty("alarmEventActions", Required = Required.Always)]
        [Description("Alarm events to perform actions on")]
        public AlarmEventAction[] AlarmEventActions { get; set; }
    }
}
