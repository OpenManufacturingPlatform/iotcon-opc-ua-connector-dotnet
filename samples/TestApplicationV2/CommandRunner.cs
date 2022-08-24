// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationV2;
using ApplicationV2.Models.Reads;
using Microsoft.Extensions.Hosting;

namespace TestApplicationV2
{
    internal class CommandRunner : BackgroundService
    {
        private readonly IOmpOpcUaClient ompOpcUaClient;

        public CommandRunner(IOmpOpcUaClient ompOpcUaClient)
        {
            this.ompOpcUaClient = ompOpcUaClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var commandCollection = new ReadCommandCollection();
            commandCollection.EndpointUrl = "";
            var readCommand = new ReadCommand
            {
                DoRegisteredRead = false,
                NodeId = ""
            };
            var resulst = ompOpcUaClient.ReadValuesAsync(commandCollection, stoppingToken);
        }
    }
}
