// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using OMP.Connector.Application.Providers.Commands;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Domain.Schema.Enums;
using OMP.Connector.Domain.Schema.Request.Control;

namespace OMP.Connector.Application.Tests.TestSetup
{
    internal static class ReadProviderSetup
    {
        internal static ReadProvider CreateReadProvider(
            IOpcSession opcSession,
            List<string> nodes,
            int readBatchSize)
        {
            var commands = nodes.Select(nodes =>
                new ReadRequest()
                {
                    NodeId = nodes,
                    OpcUaCommandType = OpcUaCommandType.Read
                });

            var opcUaSettings = Substitute.For<IOptions<ConnectorConfiguration>>();
            opcUaSettings.Value.Returns(_ => new ConnectorConfiguration
            {
                OpcUa = new OpcUaConfiguration
                {
                    ReadBatchSize = readBatchSize
                }
            });

            var logger = Substitute.For<ILogger<ReadProvider>>();
            var mapper = Substitute.For<IMapper>();

            var readProvider = new ReadProvider(
                commands,
                opcSession,
                opcUaSettings,
                mapper,
                logger
            );

            return readProvider;
        }
    }
}