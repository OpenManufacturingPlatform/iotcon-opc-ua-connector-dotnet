using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Omp.Connector.Domain.Schema.Converters.Base;
using Omp.Connector.Domain.Schema.Helpers;
using Omp.Connector.Domain.Schema.Interfaces;
using Omp.Connector.Domain.Schema.Request.Control.WriteValues;
using Omp.Connector.Domain.Schema.Request.Control.WriteValues.PrimitiveTypes.Base;

namespace Omp.Connector.Domain.Schema.Converters
{
    public class WriteRequestValueConverter : CustomJsonConverter<IWriteRequestValue>
    {
        protected override bool LoadAfterCreate => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => serializer.Serialize(writer, value, value.GetType());

        protected override IWriteRequestValue Create(System.Type objectType, JToken jToken)
        {
            return jToken.Type switch
            {
                JTokenType.Array => this.DeserializeArray(jToken as JArray),
                JTokenType.Object => this.Serializer.Deserialize<WriteRequestValue>(jToken.CreateReader()),
                JTokenType.String => this.Serializer.Deserialize<WriteRequestStringValue>(jToken.CreateReader()),
                _ => default
            };
        }

        private IWriteRequestValue DeserializeArray(JArray jArray)
        {
            return jArray.IsSimpleTypeArray()
                ? this.CreateSimpleTypeArray(jArray)
                : this.Serializer.Deserialize<WriteRequestValues>(jArray.CreateReader());
        }

        private IWriteRequestValue CreateSimpleTypeArray(JToken jToken)
        {
            var elementType = jToken.First.Type.ToSystemPrimitiveType();
            var arrayType = typeof(WriteRequestPrimitiveArray<>).MakeGenericType(new System.Type[] { elementType });
            return (IWriteRequestValue)this.Serializer.Deserialize(jToken.CreateReader(), arrayType);
        }
    }
}