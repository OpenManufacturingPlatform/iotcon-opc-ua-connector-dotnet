using System.Collections.Generic;
using Omp.Connector.Domain.Schema.Interfaces;

namespace Omp.Connector.Domain.Schema.SensorTelemetry
{
    public class SensorMeasurements : List<SensorMeasurement>, ISensorTelemetryPayloadData, IMeasurementValue
    {
    }
}