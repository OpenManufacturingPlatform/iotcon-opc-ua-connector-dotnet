using System;
using System.Linq;
using Newtonsoft.Json;

namespace Omp.Connector.Domain.Schema.Extentions
{
    public static class PropertyNameHelper
    {
        internal static string GetPropertyName(this Type objectType, string propertyName)
        {
            return !(objectType?.GetProperty(propertyName)?.GetCustomAttributes(typeof(JsonPropertyAttribute), true)
                .FirstOrDefault() is JsonPropertyAttribute jsonAttribute)
                ? propertyName
                : jsonAttribute.PropertyName;
        }
    }
}