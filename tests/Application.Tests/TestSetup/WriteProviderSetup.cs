// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Generic;
using AutoMapper;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OMP.Connector.Application.Providers.Commands;
using OMP.Connector.Domain.Schema.Request.Control;
using OMP.Connector.Infrastructure.OpcUa;

namespace OMP.Connector.Application.Tests.TestSetup
{
    internal static class WriteProviderSetup
    {
        internal static WriteProvider CreateWriteProvider(
            OpcSession opcSession,
            List<WriteRequest> writeCommands,
            IMapper mapper = null,
            ILogger<WriteProvider> logger = null)
        {

            var writeProvider = new WriteProvider(
                writeCommands,
                opcSession,
                mapper ?? Substitute.For<IMapper>(),
                logger ?? Substitute.For<ILogger<WriteProvider>>()
            );

            return writeProvider;
        }
    }
}