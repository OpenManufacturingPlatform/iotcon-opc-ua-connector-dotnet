using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Omp.Connector.Domain.Schema.Converters.Base
{
    public abstract class CustomJsonConverter<TType> : JsonConverter where TType : class
    {
        protected JsonSerializer Serializer;
        protected virtual bool LoadAfterCreate => true;

        protected abstract TType Create(System.Type objectType, JToken jToken);

        public override bool CanConvert(System.Type objectType)
            => typeof(TType).IsAssignableFrom(objectType);

        public override bool CanWrite
            => false;

        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            this.Serializer = serializer;
            var jToken = JToken.Load(reader);
            var target = this.Create(objectType, jToken);

            if (this.LoadAfterCreate && target != default)
                serializer.Populate(jToken.CreateReader(), target);

            return target;
        }

        protected TModel GetPropertyValue<TModel>(JToken jToken, string propertyName,
            StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
            => FieldExists(jToken, propertyName)
                ? jToken.ToObject<JObject>().GetValue(propertyName, stringComparison).Value<TModel>()
                : default;

        protected static bool FieldExists(JToken jToken, string propertyName,
            StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
            => jToken.ToObject<JObject>().TryGetValue(propertyName, stringComparison, out _);
    }
}