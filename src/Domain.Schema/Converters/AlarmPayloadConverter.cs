using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMP.Connector.Domain.Schema.Alarms;
using OMP.Connector.Domain.Schema.Converters.Base;
using OMP.Connector.Domain.Schema.Extenions;

namespace OMP.Connector.Domain.Schema.Converters
{
    public class AlarmPayloadConverter : CustomJsonConverter<AlarmPayload>
    {        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => serializer.Serialize(writer, value, value.GetType());

        protected override AlarmPayload Create(System.Type objectType, JToken jToken)
        {
            var propertyName = typeof(AlarmPayload).GetPropertyName(nameof(AlarmPayload.Data));
            var dataProperty = this.GetPropertyValue<dynamic>(jToken, propertyName);
            var sensorTelemetryPayload = new AlarmPayload { Data = new AlarmEventData() };

            return sensorTelemetryPayload;
        }
    }
}