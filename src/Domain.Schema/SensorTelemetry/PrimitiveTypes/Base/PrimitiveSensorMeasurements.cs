using System;
using System.Collections.Generic;
using Omp.Connector.Domain.Schema.Interfaces;

namespace Omp.Connector.Domain.Schema.SensorTelemetry.PrimitiveTypes.Base
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