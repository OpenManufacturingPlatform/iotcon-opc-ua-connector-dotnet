using System.Collections.Generic;
using OMP.Connector.Domain.Schema.SensorTelemetry.PrimitiveTypes.Base;

namespace OMP.Connector.Domain.Schema.SensorTelemetry.PrimitiveTypes
{
    public class DoubleSensorMeasurements : PrimitiveSensorMeasurements<double>
    {
        public DoubleSensorMeasurements(IEnumerable<double> items) : base(items) { }
    }
}
