using System;
using System.Collections.Generic;
using OMP.Connector.Domain.Schema.Interfaces;

namespace OMP.Connector.Domain.Schema.SensorTelemetry.PrimitiveTypes.Base
{
    public class PrimitiveSensorMeasurements<TType> : List<TType>, IMeasurementValue
        where TType: IComparable, IEquatable<TType>
    {
        public PrimitiveSensorMeasurements(IEnumerable<TType> items)
        {
            this.AddRange(items);
        }
    }
}