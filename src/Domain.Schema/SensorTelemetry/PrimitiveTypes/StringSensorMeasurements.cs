// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Generic;
using OMP.Connector.Domain.Schema.SensorTelemetry.PrimitiveTypes.Base;

namespace OMP.Connector.Domain.Schema.SensorTelemetry.PrimitiveTypes
{
    public class StringSensorMeasurements : PrimitiveSensorMeasurements<string>
    {
        public StringSensorMeasurements(IEnumerable<string> items) : base(items) { }
    }
}
