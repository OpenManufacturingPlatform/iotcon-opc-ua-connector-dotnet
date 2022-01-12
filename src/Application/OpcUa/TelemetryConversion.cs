using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using OMP.Connector.Domain;
using OMP.Connector.Domain.Models.OpcUa.Attributes;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.SensorTelemetry;
using OMP.Connector.Domain.Schema.SensorTelemetry.PrimitiveTypes.Base;
using Opc.Ua;

namespace OMP.Connector.Application.OpcUa
{
    internal static class TelemetryConversion
    {
        private const string ArrayPostfix = "[]";

        #region Conversion methods

        internal static IMeasurementValue ConvertToMeasurement(object value, string elementType, IMapper mapper)
        {
            if (IsPrimitiveStringOrNull(value))
            {
                if (value is DateTime dt)
                    return new SensorMeasurementString(dt.ToString(Constants.DateFormat));
                else
                    return new SensorMeasurementString(value?.ToString());
            }

            if (!(value is IEnumerable<string>) && value is IEnumerable<object> list)
                return ConvertToMeasurements(list as IList, elementType, mapper);

            if (value is Array array)
                return ConvertArrayOfPrimitivesToMeasurements(array, mapper);

            if (value is ExtensionObject eo)
                return ConvertExtensionObjectToMeasurements(eo);

            return ConvertObjectToMeasurements(value, mapper);
        }

        internal static SensorMeasurements ConvertToMeasurements(IList list, string elementType, IMapper mapper)
        {
            var measurements = new SensorMeasurements();
            for (int i = 0; i < list.Count; i++)
            {
                var measurement = new SensorMeasurement() { Key = i.ToString() };
                var item = list[i];
                if (item == null)
                {
                    measurement.DataType = elementType;
                    measurements.Add(measurement);
                    continue;
                }

                var itemType = item.GetType();
                measurement.DataType = typeof(IBaseComplexType).IsAssignableFrom(itemType)
                    ? FormatStructTypeName(itemType.Name, false)
                    : itemType.Name;

                measurement.Value = ConvertToMeasurement(item, itemType.GetElementType()?.ToString(), mapper);
                measurements.Add(measurement);
            }
            return measurements;
        }

        internal static IMeasurementValue ConvertArrayOfPrimitivesToMeasurements(Array array, IMapper mapper)
        {
            var elementType = array.GetType().GetElementType();
            if (elementType == typeof(Uuid))
            {
                array = mapper.Map<Guid[]>(array);
                elementType = typeof(Guid);
            }

            var type = typeof(PrimitiveSensorMeasurements<>).MakeGenericType(new[] { elementType });
            return (IMeasurementValue)Activator.CreateInstance(type, array);
        }

        internal static SensorMeasurements ConvertExtensionObjectToMeasurements(ExtensionObject value)
        {
            //TODO: Handle variants and numbers
            throw new NotImplementedException("Outstanding implementation required for Variants and Number data types.");
        }

        internal static SensorMeasurements ConvertObjectToMeasurements(object value, IMapper mapper)
        {
            var measurements = new SensorMeasurements();
            var type = value.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var property in properties)
            {
                var propVal = ConvertProperty(property.GetValue(value), mapper);
                var isArray = typeof(Array).IsAssignableFrom(property.PropertyType);
                var propType = isArray ? property.PropertyType.GetElementType() : property.PropertyType;
                propType ??= property.PropertyType;

                measurements.Add(new SensorMeasurement()
                {
                    DataType = typeof(IBaseComplexType).IsAssignableFrom(propType)
                        ? FormatStructTypeName(propType.Name, isArray)
                        : property.PropertyType.Name,
                    Key = property.Name,
                    Value = ConvertToMeasurement(propVal, property.PropertyType.GetElementType()?.Name, mapper)
                });
            }
            return measurements;
        }

        internal static object ConvertProperty(object propertyValue, IMapper mapper)
        {
            if (propertyValue is null)
                return null;

            if (propertyValue is IList list)
            {
                var newList = new List<object>();
                foreach (var item in list)
                {
                    newList.Add(ConvertProperty(item, mapper));
                }
                return newList;
            }

            var typeMap = mapper.ConfigurationProvider
                    .GetAllTypeMaps()
                    .FirstOrDefault(x => x.SourceType == propertyValue.GetType());

            return typeMap != null ? mapper.Map(propertyValue, typeMap.SourceType, typeMap.DestinationType) : propertyValue;
        }

        #endregion

        internal static string GetDataTypeName(Opc.Ua.TypeInfo typeInfo, string elementType, OpcDataValue dataValue)
            => GetDataTypeName(typeInfo.BuiltInType, typeInfo.ValueRank, elementType, dataValue.Value);

        internal static string GetDataTypeName(BuiltInType type, int valueRank, string elementType, object value)
        {
            var dataType = type.ToString();

            if (type == BuiltInType.ExtensionObject)
            {
                var isArray = IsArray(valueRank);
                var genericType = isArray ? elementType : value.GetType().Name;
                dataType = FormatStructTypeName(genericType, isArray);
            }
            else if (IsArray(valueRank))
            {
                dataType += ArrayPostfix;
            }
            return dataType;
        }

        internal static bool IsArray(int rank) => rank > 0;

        private static bool IsPrimitiveStringOrNull(object value)
            => value == null
                || value.GetType().IsPrimitive
                || value.GetType().IsValueType
                || value is string;

        private static string FormatStructTypeName(string genericType, bool isArray)
        {
            var dataType = $"{Constants.StructType}<{genericType}>";
            if (isArray) { dataType += ArrayPostfix; }
            return dataType;
        }
    }
}