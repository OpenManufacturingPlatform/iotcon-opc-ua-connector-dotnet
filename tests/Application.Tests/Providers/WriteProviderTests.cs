using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;
using OMP.Connector.Application.Tests.TestSetup;
using OMP.Connector.Domain.Models.Command.Requests.Basic.NodeCommands;
using OMP.Connector.Domain.Schema.Enums;
using OMP.Connector.Domain.Schema.Request.Control.WriteValues;
using Opc.Ua;
using WriteRequest = OMP.Connector.Domain.Schema.Request.Control.WriteRequest;
using WriteResponse = OMP.Connector.Domain.Schema.Responses.Control.WriteResponse;

namespace OMP.Connector.Application.Tests.Providers
{
    [TestFixture]
    public class WriteProviderTests
    {
        [Test]
        public void Should_Return_Bad_Response_When_Invalid_NodeId_Provided()
        {
            // Arrange
            var simulateInvalidNode = true;
            var nrOfWriteRequests = 1;
            var testWriteRequests = this.GenerateDummyWriteRequests(nrOfWriteRequests);
            var opcSession = SetupOpcSessionForWrite.CreateOpcSession();

            var mapper = Substitute.For<IMapper>();
            mapper.Map<WriteRequestWrapper>(Arg.Any<WriteRequest>()).Returns(CallMapWriteRequest());
            mapper.Map<List<WriteValue>>(Arg.Any<List<WriteRequestWrapper>>()).Returns(CallMapListWriteRequestWrapper(simulateInvalidNode));

            var writeProvider = WriteProviderSetup.CreateWriteProvider(opcSession, testWriteRequests, mapper);

            // Test
            var responses = writeProvider.ExecuteAsync().GetAwaiter().GetResult();

            // Verify
            Assert.NotNull(responses);
            Assert.That(responses.Count(), Is.EqualTo(1));
            responses.First().As<WriteResponse>().Message.Should().Be(nameof(StatusCodes.BadNodeIdInvalid));
            opcSession.Session.ReceivedWithAnyArgs(1).Write(default, default, out _, out _);
        }

        [Test]
        public void Should_Return_Good_Response_When_Valid_NodeId_Provided()
        {
            // Arrange
            var nrOfWriteRequests = 1;
            var testWriteRequests = this.GenerateDummyWriteRequests(nrOfWriteRequests);
            var opcSession = SetupOpcSessionForWrite.CreateOpcSession();

            var mapper = Substitute.For<IMapper>();
            mapper.Map<WriteRequestWrapper>(Arg.Any<WriteRequest>()).Returns(CallMapWriteRequest());
            mapper.Map<List<WriteValue>>(Arg.Any<List<WriteRequestWrapper>>()).Returns(CallMapListWriteRequestWrapper());

            var writeProvider = WriteProviderSetup.CreateWriteProvider(opcSession, testWriteRequests, mapper);

            // Test
            var responses = writeProvider.ExecuteAsync().GetAwaiter().GetResult();

            // Verify
            Assert.NotNull(responses);
            Assert.That(responses.Count(), Is.EqualTo(1));
            responses.First().As<WriteResponse>().Message.Should().Be(nameof(StatusCodes.Good));
            opcSession.Session.ReceivedWithAnyArgs(1).Write(default, default, out _, out _);
        }

        private List<WriteRequest> GenerateDummyWriteRequests(int nrOfRequests)
        {
            var writeRequests = new List<WriteRequest>();
            for (var i = 1; i <= nrOfRequests; i++)
            {
                var command = new WriteRequest()
                {
                    NodeId = i.ToString(),
                    OpcUaCommandType = OpcUaCommandType.Write,
                    Value = new WriteRequestValue()
                    {
                        DataType = "String",
                        Value = new WriteRequestStringValue($"Test Value {i}")
                    }
                };
                writeRequests.Add(command);
            }

            return writeRequests;
        }

        private static Func<CallInfo, WriteRequestWrapper> CallMapWriteRequest()
        {
            return args =>
            {
                var writeRequest = (WriteRequest)args[0];

                var writeRequestValue = (WriteRequestValue)(writeRequest.Value);
                return new WriteRequestWrapper()
                {
                    DataType = writeRequestValue.DataType,
                    NodeId = writeRequest.NodeId,
                    Value = writeRequestValue.Value
                };
            };
        }

        private static Func<CallInfo, List<WriteValue>> CallMapListWriteRequestWrapper(bool simulateInvalidNode = false)
        {
            return args =>
            {
                var writeValues = new List<WriteValue>();

                var writeRequests = (List<WriteRequestWrapper>)args[0];
                foreach (var writeRequest in writeRequests)
                {
                    writeValues.Add(new WriteValue()
                    {
                        AttributeId = Attributes.Value,
                        Value = new DataValue()
                        {
                            StatusCode = simulateInvalidNode ? StatusCodes.BadNodeIdInvalid : StatusCodes.Good,
                            Value = simulateInvalidNode ? null : writeRequest.Value.ToString()
                        }
                    });
                }

                return writeValues;
            };
        }
    }
}