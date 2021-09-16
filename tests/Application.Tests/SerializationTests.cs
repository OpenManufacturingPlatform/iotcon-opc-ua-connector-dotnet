using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using OMP.Connector.Domain.Schema.Enums;
using OMP.Connector.Domain.Schema.Factories;
using OMP.Connector.Domain.Schema.MetaData.Message;
using OMP.Connector.Domain.Schema.Responses;
using OMP.Connector.Domain.Schema.Responses.Base;
using OMP.Connector.Domain.Schema.Responses.Control;
using OMP.Connector.Domain.Schema.SensorTelemetry;
using OMP.Connector.Domain.Schema.SensorTelemetry.PrimitiveTypes;

namespace OMP.Connector.Application.Tests
{
    [TestFixture]
    public class SerializationTests
    {
        private const string SchemaContainerPath = "https://iotpebsdmstorage.blob.core.windows.net/schemas/";

        [Test]
        public void Should_Deserialize_Command_Response_Message()
        {
            // Arrange
            var originalObj = this.CreateEmptyCommandResponse();
            originalObj.Payload.Responses = new List<CommandResponse>()
                {
                    new ReadResponse()
                    {
                        DataType = "String",
                        Message = string.Empty,
                        NodeId = string.Empty,
                        OpcUaCommandType = OpcUaCommandType.Read,
                        Value = new SensorMeasurementString(string.Empty)
                    }
                };

            // Test
            var json = JsonConvert.SerializeObject(originalObj);
            var responseObj = JsonConvert.DeserializeObject<OMP.Connector.Domain.Schema.Messages.CommandResponse>(json);

            // Verify
            Assert.NotNull(responseObj);
            Assert.True(responseObj.Payload.Responses.Any());
        }

        [Test]
        public void Should_Deserialize_DoubleArray()
        {
            // Arrange
            var originalObj = this.CreateEmptyCommandResponse();
            originalObj.Payload.Responses = new List<CommandResponse>()
            {
                new ReadResponse()
                {
                    DataType = "String",
                    Message = string.Empty,
                    NodeId = string.Empty,
                    OpcUaCommandType = OpcUaCommandType.Read,
                    Value = new SensorMeasurements()
                    {
                        new SensorMeasurement()
                        {
                            DataType = "Double[]",
                            Key = "nodeid1",
                            Value = new DoubleSensorMeasurements(new []{Double.MinValue, Double.MaxValue})
                        }

                    }
                }
            };

            var jsonStr = JsonConvert.SerializeObject(originalObj);

            // Test
            var responseObj = JsonConvert.DeserializeObject<Domain.Schema.Messages.CommandResponse>(jsonStr);

            // Verify
            Assert.NotNull(responseObj);
            Assert.True(responseObj.Payload.Responses.Any());
        }

        private Domain.Schema.Messages.CommandResponse CreateEmptyCommandResponse()
        {
            var cmdResponse = ModelFactory.CreateInstance<Domain.Schema.Messages.CommandResponse>(SchemaContainerPath);
            cmdResponse.Id = Guid.NewGuid().ToString();
            cmdResponse.MetaData = new MessageMetaData();
            cmdResponse.Payload = new ResponsePayload()
            {
                ResponseSource = new ResponseSource()
                {
                    Id = Guid.NewGuid().ToString(),
                    EndpointUrl = string.Empty,
                    Name = string.Empty,
                    Route = string.Empty
                },
                Responses = new List<CommandResponse>()
            };
            return cmdResponse;
        }
    }
}