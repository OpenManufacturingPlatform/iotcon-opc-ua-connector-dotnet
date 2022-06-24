// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMP.Connector.Domain.Schema.Alarms;
using OMP.Connector.Domain.Schema.Converters.Base;

namespace OMP.Connector.Domain.Schema.Converters
{
    public class AlarmPayloadConverter : CustomJsonConverter<AlarmPayload>
    {        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => serializer.Serialize(writer, value, value.GetType());

        protected override AlarmPayload Create(System.Type objectType, JToken jToken)
        {
            var alarmPayload = new AlarmPayload { Data = new AlarmEventData() };

            return alarmPayload;
        }
    }
}
