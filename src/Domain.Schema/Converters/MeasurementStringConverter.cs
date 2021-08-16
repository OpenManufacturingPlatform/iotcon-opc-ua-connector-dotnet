using System.ComponentModel;
using System.Globalization;
using Omp.Connector.Domain.Schema.SensorTelemetry;

namespace Omp.Connector.Domain.Schema.Converters
{
    public class MeasurementStringConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
            => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            => value is string stringValue
                ? new SensorMeasurementString(stringValue)
                : base.ConvertFrom(context, culture, value);
    }
}