using System.ComponentModel;
using OMP.Connector.Domain.Schema.Converters;
using OMP.Connector.Domain.Schema.Interfaces;

namespace OMP.Connector.Domain.Schema.Request.Control.WriteValues
{
    [TypeConverter(typeof(WriteRequestStringValueConverter))]
    public class WriteRequestStringValue : IWriteRequestValue
    {
        private readonly string _value;

        public WriteRequestStringValue(string value)
            => this._value = value;

        public static implicit operator string(WriteRequestStringValue i) => i._value;
        public static explicit operator WriteRequestStringValue(string i) => new WriteRequestStringValue(i);

        public override string ToString()
            => this._value;
    }
}