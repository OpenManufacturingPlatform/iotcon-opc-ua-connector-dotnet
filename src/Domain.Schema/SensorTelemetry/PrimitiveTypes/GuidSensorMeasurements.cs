// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using OMP.Connector.Domain.Schema.SensorTelemetry.PrimitiveTypes.Base;

namespace OMP.Connector.Domain.Schema.SensorTelemetry.PrimitiveTypes
{
    public class GuidSensorMeasurements : PrimitiveSensorMeasurements<Guid>
    {
        public GuidSensorMeasurements(IEnumerable<Guid> items) : base(items) { }
    }
}
