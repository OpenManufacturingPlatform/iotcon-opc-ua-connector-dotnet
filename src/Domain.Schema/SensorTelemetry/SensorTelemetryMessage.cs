// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel;
using OMP.Connector.Domain.Schema.Base;

namespace OMP.Connector.Domain.Schema.SensorTelemetry
{
    [Description("Definition of Sensor Telemetry Messages")]
    public class SensorTelemetryMessage : Message<SensorTelemetryPayload> { }
}