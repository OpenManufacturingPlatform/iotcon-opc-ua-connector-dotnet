// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.Core;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Infrastructure.OpcUa;
using OMP.Connector.Tests.Support.Fakes;
using Opc.Ua;

namespace OMP.Connector.Application.Tests.TestSetup
{
    internal static class SetupOpcSessionForRead
    {
        internal static IOpcSession CreateOpcSession()
        {
            var connectorConfiguration = Substitute.For<IOptions<ConnectorConfiguration>>();
            connectorConfiguration.Value.Returns(_ => new ConnectorConfiguration
            {
                OpcUa = new OpcUaConfiguration()
            });
            var applicationConfiguration = Substitute.For<ApplicationConfiguration>();
            var mapper = Substitute.For<IMapper>();
            var loggerFactory = Substitute.For<ILoggerFactory>();
            var identityProvider = Substitute.For<IUserIdentityProvider>();

            var opcSession = Substitute.For<OpcSession>(
                connectorConfiguration,
                loggerFactory,
                applicationConfiguration,
                mapper,
                identityProvider
            );

            opcSession.Session = FakeOpcUaSession.Create<FakeOpcUaSession>();

            opcSession.Session
                .Read(default, default, default, default, out _, out _)
                .ReturnsForAnyArgs(CallRead());

            return opcSession;
        }

        private static Func<CallInfo, ResponseHeader> CallRead()
        {
            return args =>
            {
                var inputCollection = (ReadValueIdCollection)args[3];
                var actualValues = new DataValueCollection();
                foreach (var nodeToRead in inputCollection)
                {
                    //node value simulated by taking number in nodeid
                    var nodeId = nodeToRead.NodeId.ToString();
                    var indexOfNumber = nodeId.LastIndexOf("=") + 1;
                    var value = nodeId[indexOfNumber..];
                    actualValues.Add(new DataValue(value));
                };
                args[4] = actualValues;
                return new ResponseHeader();
            };
        }
    }
}
