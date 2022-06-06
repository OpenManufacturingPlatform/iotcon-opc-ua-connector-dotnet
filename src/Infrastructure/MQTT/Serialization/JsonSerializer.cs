// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Newtonsoft.Json;

namespace OMP.Connector.Infrastructure.MQTT.Serialization
{
    public class JsonSerializer : ISerializer
    {
        protected readonly JsonSerializerSettings JsonSerializerSettings =
            new()
            {
                NullValueHandling = NullValueHandling.Include,
                DateParseHandling = DateParseHandling.None
            };

        public T Deserialize<T>(string value)
            => JsonConvert.DeserializeObject<T>(value, JsonSerializerSettings);


        public string Serialize(object value)
            => JsonConvert.SerializeObject(value, JsonSerializerSettings);
    }
}
