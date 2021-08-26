using System.Collections.Generic;
using OMP.Connector.Domain.Schema.SensorTelemetry.PrimitiveTypes.Base;

namespace OMP.Connector.Domain.Schema.SensorTelemetry.PrimitiveTypes
{
    public class IntegerSensorMeasurements : PrimitiveSensorMeasurements<int>
    {
        public IntegerSensorMeasurements(IEnumerable<int> items) : base(items) { }
    }
}
