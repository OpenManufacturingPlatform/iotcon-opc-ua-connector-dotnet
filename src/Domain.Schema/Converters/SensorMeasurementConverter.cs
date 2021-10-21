﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OMP.Connector.Domain.Schema.Converters.Base;
using OMP.Connector.Domain.Schema.Helpers;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.SensorTelemetry;
using OMP.Connector.Domain.Schema.SensorTelemetry.PrimitiveTypes.Base;
using System.Collections.Generic;
using System.Linq;

namespace OMP.Connector.Domain.Schema.Converters
{
    public class SensorMeasurementConverter : CustomJsonConverter<IMeasurementValue>
    {
        public override bool CanWrite => true;

        protected override bool LoadAfterCreate => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if(value is List<SensorMeasurement> list)
            {
                foreach (var item in list)
                {
                    if(item.Key.Equals("Value"))
                    {

                    }
                    else
                    {
                        serializer.Serialize(writer, value, value.GetType());
                    }
                }
            }
            else
            {
                serializer.Serialize(writer, value, value.GetType());
            }
        }

        protected override IMeasurementValue Create(System.Type objectType, JToken jToken)
        {
            return jToken.Type switch
            {
                JTokenType.Array => this.DeserializeArray(jToken as JArray),
                JTokenType.Object => this.Serializer.Deserialize<SensorMeasurement>(jToken.CreateReader()),
                JTokenType.String => this.Serializer.Deserialize<SensorMeasurementString>(jToken.CreateReader()),
                _ => default
            };
        }

        private IMeasurementValue DeserializeArray(JArray jArray)
        {
            return jArray.IsSimpleTypeArray()
                ? this.CreateSimpleTypeArray(jArray)
                : this.Serializer.Deserialize<SensorMeasurements>(jArray.CreateReader());
        }

        private IMeasurementValue CreateSimpleTypeArray(JToken jToken)
        {
            var elementType = jToken.First.Type.ToSystemPrimitiveType();
            var arrayType = typeof(PrimitiveSensorMeasurements<>).MakeGenericType(new System.Type[] { elementType });
            return (IMeasurementValue)this.Serializer.Deserialize(jToken.CreateReader(), arrayType);
        }
    }
}