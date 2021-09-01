using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OMP.Connector.Domain.Json.Base
{
    public abstract class CustomConverter<T> : JsonConverter
    {
        protected abstract T Create(Type objectType, JObject jObject);

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override bool CanWrite => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var target = this.Create(objectType, jObject);
            serializer.Populate(jObject.CreateReader(), target);
            return target;
        }

        protected TModel GetPropertyValue<TModel>(JObject jObject, string propertyName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            return FieldExists(jObject, propertyName)
                ? jObject.GetValue(propertyName, stringComparison).Value<TModel>()
                : default;
        }

        protected static bool FieldExists(JObject jObject, string propertyName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            return jObject.TryGetValue(propertyName, stringComparison, out _);
        }
    }
}