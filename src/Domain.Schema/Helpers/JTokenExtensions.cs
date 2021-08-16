using System;
using Newtonsoft.Json.Linq;

namespace Omp.Connector.Domain.Schema.Helpers
{
    public static class JTokenExtensions
    {
        public static bool IsSimpleTypeArray(this JArray jToken)
        {
            var nextTokenType = jToken?.First?.Type;
            return nextTokenType == JTokenType.String ||
                   nextTokenType == JTokenType.Boolean ||
                   nextTokenType == JTokenType.Integer ||
                   nextTokenType == JTokenType.Date ||
                   nextTokenType == JTokenType.Float ||
                   nextTokenType == JTokenType.Guid;
        }

        public static Type ToSystemPrimitiveType(this JTokenType jTokenType)
        {
            return jTokenType switch
            {
                JTokenType.String => typeof(string),
                JTokenType.Integer => typeof(decimal),
                JTokenType.Boolean => typeof(bool),
                JTokenType.Date => typeof(DateTime),
                JTokenType.Float => typeof(Double),
                JTokenType.Guid => typeof(Guid),
                _ => default
            };
        }
    }
}
