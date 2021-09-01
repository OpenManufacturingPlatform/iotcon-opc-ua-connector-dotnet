using System.Collections.Generic;
using OMP.Connector.Domain.Schema.Interfaces;

namespace OMP.Connector.Domain.Schema.SensorTelemetry
{
    public class SensorMeasurements : List<SensorMeasurement>, ISensorTelemetryPayloadData, IMeasurementValue
    {
    }
}