using System.Collections.Generic;
using OMP.Connector.Domain.Schema.SensorTelemetry.PrimitiveTypes.Base;

namespace OMP.Connector.Domain.Schema.SensorTelemetry.PrimitiveTypes
{
    public class FloatSensorMeasurements : PrimitiveSensorMeasurements<float>
    {
        public FloatSensorMeasurements(IEnumerable<float> items) : base(items) { }
    }
}
