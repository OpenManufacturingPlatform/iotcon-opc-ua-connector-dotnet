// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel;
using System.Globalization;
using OMP.Connector.Domain.Schema.Request.Control.WriteValues;

namespace OMP.Connector.Domain.Schema.Converters
{
    public class WriteRequestStringValueConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
            => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            => value is string stringValue
                ? new WriteRequestStringValue(stringValue)
                : base.ConvertFrom(context, culture, value);
    }
}