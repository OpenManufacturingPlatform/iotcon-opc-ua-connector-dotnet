using System;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.Core;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Infrastructure.OpcUa.Reconnect;
using OMP.Connector.Infrastructure.OpcUa;
using OMP.Connector.Tests.Support.Fakes;
using Opc.Ua;

namespace OMP.Connector.Application.Tests.TestSetup
{
    internal static class SetupOpcSessionForWrite
    {
        internal static OpcSession CreateOpcSession()
        {
            var connectorConfiguration = Substitute.For<IOptions<ConnectorConfiguration>>();
            connectorConfiguration.Value.Returns(_ => new ConnectorConfiguration
            {
                OpcUa = new OpcUaConfiguration()
            });
            var opcSessionReconnectHandlerFactory = Substitute.For<IOpcSessionReconnectHandlerFactory>();
            var applicationConfiguration = Substitute.For<ApplicationConfiguration>();
            var mapper = Substitute.For<IMapper>();
            var logger = Substitute.For<ILoggerFactory>();

            var opcSession = Substitute.For<OpcSession>(
                connectorConfiguration,
                opcSessionReconnectHandlerFactory,
                logger,
                applicationConfiguration,
                mapper
            );

            opcSession.Session = FakeOpcUaSession.Create<FakeOpcUaSession>();

            opcSession.Session
                .Read(default, default, default, default, out _, out _)
                .ReturnsForAnyArgs(CallRead());

            opcSession.Session
                .Write(default, default, out _, out _)
                .ReturnsForAnyArgs(CallWrite());

            return opcSession;
        }

        private static Func<CallInfo, ResponseHeader> CallRead(bool simulateInvalidNode = false)
        {
            return args =>
            {
                var actualValues = new DataValueCollection();

                var inputCollection = (ReadValueIdCollection)args[3];

                foreach (var nodeToRead in inputCollection)
                {
                    if(nodeToRead.AttributeId == Attributes.DataType)
                    {
                        actualValues.Add(new DataValue()
                        {
                            StatusCode = simulateInvalidNode ? StatusCodes.BadNodeIdInvalid : StatusCodes.Good,
                            Value = simulateInvalidNode ? null : new DataValue() { Value = 12 } //nodeId for OPC.UA.String is 12
                        });
                    }

                    if (nodeToRead.AttributeId == Attributes.ValueRank)
                    {
                        actualValues.Add(new DataValue()
                        {
                            StatusCode = simulateInvalidNode ? StatusCodes.BadNodeIdInvalid : StatusCodes.Good,
                            Value = simulateInvalidNode ? null : new DataValue(){Value = -1}
                        });
                    }
                };
                args[4] = actualValues;
                return new ResponseHeader();
            };
        }

        private static Func<CallInfo, ResponseHeader> CallWrite()
        {
            return args =>
            {
                var actualValues = new StatusCodeCollection();

                var inputCollection = (WriteValueCollection)args[1];

                foreach (var nodeToWrite in inputCollection)
                {
                    actualValues.Add(nodeToWrite.Value.StatusCode);
                };
                args[2] = actualValues;
                return new ResponseHeader();
            };
        }
    }
}