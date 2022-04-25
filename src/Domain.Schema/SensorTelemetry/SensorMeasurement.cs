// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Converters;
using OMP.Connector.Domain.Schema.Interfaces;

namespace OMP.Connector.Domain.Schema.SensorTelemetry
{
    [Description("Sensor measurement")]
    public class SensorMeasurement : ISensorTelemetryPayloadData, IMeasurementValue
    {
        [JsonProperty("key", Required = Required.Always)]
        [Description("Key for sensor value")]
        public string Key { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Node Name")]
        public string Name { get; set; }

        [JsonConverter(typeof(SensorMeasurementConverter))]
        [JsonProperty("value", Required = Required.AllowNull)]
        [Description("Measurement value")]
        public IMeasurementValue Value { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Status code for measurement value")]
        public string Status { get; set; }

        [Timestamp]
        [Attributes.Examples.TimeStampExamples]
        [JsonProperty("lastChangeTimestamp", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Timestamp in ISO8601 format. UTC should be used whenever possible")]
        public DateTime? LastChangeTimestamp { get; set; }

        [Timestamp]
        [Attributes.Examples.TimeStampExamples]
        [JsonProperty("measurementTimestamp", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Timestamp in ISO8601 format. UTC should be used whenever possible")]
        public DateTime? MeasurementTimestamp { get; set; }

        [JsonProperty("dataType", NullValueHandling = NullValueHandling.Ignore)]
        [Description("Data Type of sensor value")]
        public string DataType { get; set; }

        [JsonProperty("route", NullValueHandling = NullValueHandling.Ignore)]
        [Description("The route of the node")]
        public string Route { get; set; }
    }
}