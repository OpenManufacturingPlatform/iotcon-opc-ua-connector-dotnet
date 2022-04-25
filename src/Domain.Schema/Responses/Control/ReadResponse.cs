// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.ComponentModel;
using Newtonsoft.Json;
using OMP.Connector.Domain.Schema.Converters;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.Responses.Control.Base;

namespace OMP.Connector.Domain.Schema.Responses.Control
{
    public class ReadResponse : NodeCommandResponse
    {
        [JsonProperty("dataType", Required = Required.Always)]
        [Description("Data type of the value")]
        public string DataType { get; set; }

        [JsonConverter(typeof(SensorMeasurementConverter))]
        [JsonProperty("value", Required = Required.AllowNull)]
        [Description("Value of the node")]
        public IMeasurementValue Value { get; set; }
    }
}