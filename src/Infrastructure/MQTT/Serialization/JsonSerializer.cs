using Newtonsoft.Json;

namespace OMP.Connector.Infrastructure.MQTT.Serialization
{
    public class JsonSerializer : ISerializer
    {
        protected readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Include };

        public T Deserialize<T>(string value)
            => JsonConvert.DeserializeObject<T>(value, JsonSerializerSettings);


        public string Serialize(object value)
            => JsonConvert.SerializeObject(value, JsonSerializerSettings);
    }
}
