// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using OMP.Connector.Domain.Schema.Enums;
using OMP.Connector.Domain.Schema.Factories;
using OMP.Connector.Domain.Schema.Interfaces;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Domain.Schema.MetaData.Message;
using OMP.Connector.Domain.Schema.Request;
using OMP.Connector.Domain.Schema.Request.Control;
using OMP.Connector.Domain.Schema.Responses;

namespace OMP.Connector.Application.Tests.TestSetup
{
    internal static class CommandHelper
    {
        internal static CommandResponse CreateCommandResponse(
            string schemaUrl,
            CommandRequest command,
            string expectedServerName = null,
            string expectedServerRoute = null,
            params ICommandResponse[] commandResponses)
        {
            var message = ModelFactory.CreateInstance<CommandResponse>(schemaUrl);
            message.MetaData = new MessageMetaData()
            {
                CorrelationIds = new List<string>()
            };
            message.Payload = new ResponsePayload
            {
                ResponseSource = new ResponseSource
                {
                    Id = Guid.NewGuid().ToString(),
                    EndpointUrl = command.Payload.RequestTarget.EndpointUrl,
                    Name = expectedServerName ?? string.Empty,
                    Route = expectedServerRoute ?? string.Empty
                },
                Responses = new List<ICommandResponse>(commandResponses)
            };
            return message;
        }

        internal static CommandRequest CreateCommandRequest(OpcUaCommandType commandType, string nodeId, string schemaUrl)
        {
            var message = ModelFactory.CreateInstance<CommandRequest>(schemaUrl);
            message.Id = Guid.NewGuid().ToString();
            message.MetaData = new MessageMetaData();
            message.Payload = new RequestPayload
            {
                RequestTarget = new RequestTarget
                {
                    EndpointUrl = "Unit Test Endpoint"
                },
                Requests = new[]
                {
                    new ReadRequest
                    {
                        NodeId = nodeId,
                        OpcUaCommandType = commandType
                    }
                }
            };
            return message;
        }

        internal static bool AreEqual(CommandResponse expected, CommandResponse actual)
        {
            actual.Namespace.Should().BeEquivalentTo(expected.Namespace);
            actual.Schema.Should().BeEquivalentTo(expected.Schema);
            actual.Payload.ResponseSource.EndpointUrl.Should().BeEquivalentTo(expected.Payload.ResponseSource.EndpointUrl);
            actual.Payload.ResponseSource.Name.Should().BeEquivalentTo(expected.Payload.ResponseSource.Name);
            actual.Payload.ResponseSource.Route.Should().BeEquivalentTo(expected.Payload.ResponseSource.Route);
            actual.Payload.Responses.ResponsesAreTheSame(expected.Payload.Responses);
            actual.MetaData.CorrelationIds.CorrelationIdsAreTheSame(expected.MetaData.CorrelationIds);

            return true;
        }

        internal static void ResponsesAreTheSame(this IEnumerable<ICommandResponse> actualResponses, IEnumerable<ICommandResponse> expectedResponses)
        {
            var actual = actualResponses.ToList();
            var expected = expectedResponses.ToList();
            actual.Should().HaveCount(expected.Count());
            for (var i = 0; i < actual.Count(); i++)
            {
                var aResponse = actual.Skip(i).First();
                var eResponse = expected.Skip(i).First();
                //TODO: find better way than serialization to compare likeness
                JsonConvert.SerializeObject(aResponse).Should().BeEquivalentTo(JsonConvert.SerializeObject(eResponse));
            }
        }

        internal static void CorrelationIdsAreTheSame(this IEnumerable<string> actualCorrelationIds, IEnumerable<string> expectedCorrelationIds)
        {
            var actual = actualCorrelationIds.ToList();
            var expected = expectedCorrelationIds.ToList();

            actual.Should().HaveCount(expected.Count());
            for (var i = 0; i < expected.Count(); i++)
            {
                var actualCorrelationId = actual.Skip(i).First();
                var expectedCorrelationId = expected.Skip(i).First();
                Assert.AreEqual(expectedCorrelationId, actualCorrelationId);
                actualCorrelationId.Should().BeSameAs(expectedCorrelationId);
            }
        }
    }
}
