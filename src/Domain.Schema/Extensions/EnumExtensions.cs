using System;
using System.Linq;
using System.Runtime.Serialization;

namespace OMP.Connector.Domain.Schema.Extenions
{
    public static class EnumExtensions
    {
        public static string GetMemberValue(this Enum @enum)
        {
            var attr =
                @enum.GetType().GetMember(@enum.ToString()).FirstOrDefault()?.GetCustomAttributes(false)
                    .OfType<EnumMemberAttribute>().FirstOrDefault();

            return attr == null ? @enum.ToString() : attr.Value;
        }
    }
}