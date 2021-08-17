using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Omp.Connector.Domain.Schema.Converters.Base;
using Omp.Connector.Domain.Schema.Extentions;
using Omp.Connector.Domain.Schema.SensorTelemetry;

namespace Omp.Connector.Domain.Schema.Converters
{
    public class SensorPayloadConverter : CustomJsonConverter<SensorTelemetryPayload>
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => serializer.Serialize(writer, value, value.GetType());

        protected override SensorTelemetryPayload Create(System.Type objectType, JToken jToken)
        {
            var propertyName = typeof(SensorTelemetryPayload).GetPropertyName(nameof(SensorTelemetryPayload.Data));
            var dataProperty = this.GetPropertyValue<dynamic>(jToken, propertyName);
            var sensorTelemetryPayload = new SensorTelemetryPayload { Data = new SensorMeasurement() };
            if (dataProperty.GetType() == typeof(JArray))
                sensorTelemetryPayload.Data = new SensorMeasurements();

            return sensorTelemetryPayload;
        }
    }
}