using System.ComponentModel;
using OMP.Connector.Domain.Schema.Converters;
using OMP.Connector.Domain.Schema.Interfaces;

namespace OMP.Connector.Domain.Schema.SensorTelemetry
{
    [TypeConverter(typeof(MeasurementStringConverter))]
    public class SensorMeasurementString : IMeasurementValue
    {
        private readonly string _value;

        public SensorMeasurementString(string value)
            => this._value = value;

        public static implicit operator string(SensorMeasurementString i) => i._value;
        public static explicit operator SensorMeasurementString(string i) => new SensorMeasurementString(i);

        public override string ToString()
            => this._value;
    }
}