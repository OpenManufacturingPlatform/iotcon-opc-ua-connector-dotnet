// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Generic;
using OMP.Connector.Domain.Schema.SensorTelemetry.PrimitiveTypes.Base;

namespace OMP.Connector.Domain.Schema.SensorTelemetry.PrimitiveTypes
{
    public class BoolSensorMeasurements : PrimitiveSensorMeasurements<bool>
    {
        public BoolSensorMeasurements(IEnumerable<bool> items) : base(items) { }
    }
}
