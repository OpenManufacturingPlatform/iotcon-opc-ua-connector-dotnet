// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OMP.Connector.Application.OpcUa;
using OMP.Connector.Application.Tests.TestSetup;
using OMP.Connector.Domain.Schema;
using OMP.Connector.Domain.Schema.Enums;
using OMP.Connector.Domain.Schema.Factories;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Domain.Schema.MetaData.Message;
using OMP.Connector.Domain.Schema.Request;
using OMP.Connector.Domain.Schema.Request.Control;
using OMP.Connector.Domain.Schema.Responses;
using OMP.Connector.Domain.Schema.Responses.Control;
using OMP.Connector.Domain.Schema.Responses.Discovery;
using OMP.Connector.Domain.Schema.Responses.Subscription;

namespace OMP.Connector.Application.Tests
{
    [TestFixture]
    public class CommandResponseCreatorTests
    {
        [Test]
        public void Should_Create_Correct_Error_Response_From_Original_Response()
        {
            // Arrange
            var commandResponse = GetCommandResponse(OpcUaResponseStatus.Good);

            // Test
            var actualErrorResponse = CommandResponseCreator.GetErrorResponseMessage(TestConstants.SchemaUrl, commandResponse);

            // Validate
            IsValidErrorResponse(commandResponse, actualErrorResponse);
        }

        [Test]
        public void Should_Create_Correct_Error_Response_For_Response()
        {
            // Arrange
            var commandRequest = GetCommandRequest();

            // Test
            var actualErrorResponse = CommandResponseCreator.GetErrorResponseMessage(TestConstants.SchemaUrl, commandRequest);

            // Validate
            IsValidErrorResponse(commandRequest, actualErrorResponse);
        }

        [TestCase(false, OpcUaResponseStatus.Good)]
        [TestCase(true, OpcUaResponseStatus.Bad)]
        public void Should_Create_Response_With_Correct_ResponseStatus(bool atLeastOneBadResult, OpcUaResponseStatus expectedResponseStatus)
        {
            // Arrange
            var commandRequest = GetCommandRequest();
            var commandResponses = GetCommandResponses(atLeastOneBadResult);

            // Test
            var actualResponse = CommandResponseCreator.GetCommandResponseMessage(TestConstants.SchemaUrl, commandRequest, commandResponses);
            var actualResponseStatus = actualResponse.Payload.ResponseStatus;

            // Validate
            Assert.AreEqual(expectedResponseStatus, actualResponseStatus);
        }

        [Test]
        public void Should_Throw_Exception_If_schemaUrl_Not_Supplied()
        {
            // Arrange
            var commandRequest = GetCommandRequest();
            string schemaUrl = null;
            var commandResponses = GetCommandResponses(false);

            // Test
            // Validate
            Assert.Throws<ArgumentNullException>(() => { CommandResponseCreator.GetCommandResponseMessage(schemaUrl, commandRequest, commandResponses); });
        }

        [Test]
        public void Should_Throw_Exception_If_CommandRequest_Not_Supplied()
        {
            // Arrange
            CommandRequest commandRequest = null;
            var commandResponses = GetCommandResponses(false);

            // Test
            // Validate
            Assert.Throws<ArgumentNullException>(() => { CommandResponseCreator.GetCommandResponseMessage(TestConstants.SchemaUrl, commandRequest, commandResponses); });
        }

        [Test]
        public void Should_Generate_Error_Response_When_No_CommandResponses_Supplied()
        {
            // Arrange
            var commandRequest = GetCommandRequest();
            List<ICommandResponse> commandResponses = null;

            // Test
            var actualResponse = CommandResponseCreator.GetCommandResponseMessage(TestConstants.SchemaUrl, commandRequest, commandResponses);

            // Validate
            Assert.NotNull(actualResponse);
            Assert.IsEmpty(actualResponse.Payload.Responses);
            Assert.AreEqual(OpcUaResponseStatus.Bad, actualResponse.Payload.ResponseStatus);
        }

        private static bool IsValidErrorResponse(CommandResponse failedResponse, CommandResponse actualErrorResponse)
        {
            Assert.AreEqual(failedResponse.Id, actualErrorResponse.Id);
            Assert.AreEqual(failedResponse.MetaData.CorrelationIds, actualErrorResponse.MetaData.CorrelationIds);
            Assert.AreEqual(OpcUaResponseStatus.Bad, actualErrorResponse.Payload.ResponseStatus);
            Assert.AreEqual(failedResponse.Payload.ResponseSource, actualErrorResponse.Payload.ResponseSource);
            Assert.IsEmpty(actualErrorResponse.Payload.Responses);
            return true;
        }

        private static bool IsValidErrorResponse(CommandRequest commandRequest, CommandResponse actualErrorResponse)
        {
            Assert.AreNotEqual(commandRequest.Id, actualErrorResponse.Id);
            Assert.AreEqual(commandRequest.MetaData.CorrelationIds, actualErrorResponse.MetaData.CorrelationIds);
            Assert.AreEqual(OpcUaResponseStatus.Bad, actualErrorResponse.Payload.ResponseStatus);
            Assert.AreEqual(commandRequest.MetaData.DestinationIdentifiers.First(), actualErrorResponse.MetaData.SenderIdentifier);
            Assert.AreEqual(commandRequest.MetaData.SenderIdentifier, actualErrorResponse.MetaData.DestinationIdentifiers.First());
            Assert.IsEmpty(actualErrorResponse.Payload.Responses);
            return true;
        }

        private static CommandResponse GetCommandResponse(OpcUaResponseStatus responseStatus)
        {
            var commandResponse = ModelFactory.CreateInstance<CommandResponse>(TestConstants.SchemaUrl);
            commandResponse.Id = TestConstants.ExpectedResponseId;
            commandResponse.MetaData = new MessageMetaData()
            {
                CorrelationIds = new List<string>() { TestConstants.ExpectedCorrelationId },
                DestinationIdentifiers = new List<Participant>(),
                SenderIdentifier = new Participant()
            };
            commandResponse.Payload = new ResponsePayload()
            {
                ResponseStatus = responseStatus,
                ResponseSource = new ResponseSource() { EndpointUrl = TestConstants.ExpectedEndpointUrl },
                Responses = new List<ICommandResponse>()
                {
                    new WriteResponse()
                    {
                        NodeId = TestConstants.NodeId,
                        OpcUaCommandType = OpcUaCommandType.Write
                    }
                }
            };
            return commandResponse;
        }

        private static CommandRequest GetCommandRequest()
        {
            var commandRequest = ModelFactory.CreateInstance<CommandRequest>(TestConstants.SchemaUrl);
            commandRequest.Id = TestConstants.ExpectedResponseId;
            commandRequest.MetaData = new MessageMetaData()
            {
                CorrelationIds = new List<string>() { Guid.NewGuid().ToString() },
                DestinationIdentifiers = new List<Participant>()
                {
                    new Participant()
                    {
                        Id = TestConstants.ExpectedConnectorId
                    }
                },
                SenderIdentifier = new Participant()
            };
            commandRequest.Payload = new RequestPayload()
            {
                RequestTarget = new RequestTarget() { EndpointUrl = TestConstants.ExpectedEndpointUrl },
                Requests = new List<ICommandRequest>()
                {
                    new ReadRequest()
                    {
                        NodeId = TestConstants.NodeId,
                        OpcUaCommandType = OpcUaCommandType.Read
                    }
                }
            };
            return commandRequest;
        }

        private static List<ICommandResponse> GetCommandResponses(bool atLeastOneBadResult)
        {
            var goodMsg = TestConstants.ExpectedCommandGoodMessage;

            var list = new List<ICommandResponse>()
            {
                new ReadResponse()
                {
                    Message = goodMsg
                },
                new WriteResponse()
                {
                    Message = goodMsg
                },
                new BrowseResponse()
                {
                    Message = goodMsg
                },
                new CallResponse()
                {
                    Message = goodMsg
                },
                new CreateSubscriptionsResponse()
                {
                    Message = goodMsg
                },
                new RemoveSubscriptionsResponse()
                {
                    Message = goodMsg
                },
                new BrowseChildNodesFromRootResponse()
                {
                    Message = goodMsg
                },
                new BrowseChildNodesResponse()
                {
                    Message = goodMsg
                },
                new ServerDiscoveryResponse()
                {
                    Message = goodMsg
                }
            };

            list.Add(
                atLeastOneBadResult
                    ? new RemoveAllSubscriptionsResponse() { Message = TestConstants.CommandBadMessage }
                    : new RemoveAllSubscriptionsResponse() { Message = goodMsg });

            return list;
        }
    }
}