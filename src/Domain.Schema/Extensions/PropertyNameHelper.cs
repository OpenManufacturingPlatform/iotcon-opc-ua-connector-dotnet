// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Linq;
using Newtonsoft.Json;

namespace OMP.Connector.Domain.Schema.Extenions
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