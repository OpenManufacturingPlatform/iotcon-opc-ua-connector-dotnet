// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationV2;
using ApplicationV2.Models.Reads;
using ApplicationV2.Models.Subscriptions;
using ApplicationV2.Models.Writes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Ua;

namespace TestApplicationV2
{
    internal class CommandRunner : BackgroundService
    {
        const string EndPointUrl = "opc.tcp://bw09861291:52210/UA/SampleServer";
        private readonly IOmpOpcUaClient ompOpcUaClient;
        private readonly ILogger<CommandRunner> logger;

        public CommandRunner(IOmpOpcUaClient ompOpcUaClient, ILogger<CommandRunner> logger)
        {
            this.ompOpcUaClient = ompOpcUaClient;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await SubscribeToNodes(stoppingToken);
            //await PssReadTestAsync(stoppingToken);
            //await RunReadTest(stoppingToken);
            //await RunWriteTest(stoppingToken);
        }

        private async Task PssReadTestAsync(CancellationToken stoppingToken)
        {
            //opc.tcp://160.52.61.130:4840
            var commandCollection = new ReadCommandCollection();
            commandCollection.EndpointUrl = "opc.tcp://160.52.61.130:4840";
            commandCollection.Add(new ReadCommand
            {
                DoRegisteredRead = false,
                NodeId = "ns=3;s=OrderNumber"
            });
            commandCollection.Add(new ReadCommand
            {
                DoRegisteredRead = true,
                NodeId = "ns=3;i=5204"
            });

            var resulst = await ompOpcUaClient.ReadValuesAsync(commandCollection, stoppingToken);
            resulst.Switch(
                success =>
                {
                    logger.LogInformation("Read Succeeded: {results}", success.Select(s => (s.Succeeded, s.Response!.Value)));
                },
                failure =>
                {
                    logger.LogCritical("Read failed");
                });
        }

        private async Task RunReadTest(CancellationToken stoppingToken)
        {
            var commandCollection = new ReadCommandCollection();
            commandCollection.EndpointUrl = EndPointUrl;
            commandCollection.Add(new ReadCommand
            {
                DoRegisteredRead = false,
                NodeId = "ns=2;i=1585"
            });
            commandCollection.Add(new ReadCommand
            {
                DoRegisteredRead = true,
                NodeId = "ns=2;i=1587"
            });

            var resulst = await ompOpcUaClient.ReadValuesAsync(commandCollection, stoppingToken);
            resulst.Switch(
                success =>
                {
                    logger.LogInformation("Read Succeeded: {results}", success.Select(s => (s.Succeeded, s.Response!.Value)));
                },
                failure =>
                {
                    logger.LogCritical("Read failed");
                });
        }

        private async Task RunWriteTest(CancellationToken stoppingToken)
        {
            var commandCollection = new WriteCommandCollection();
            commandCollection.EndpointUrl = EndPointUrl;
            commandCollection.Add(new WriteCommand
            {
                DoRegisteredWrite = false,
                Value = new Opc.Ua.WriteValue
                {
                    NodeId = "ns=2;i=920",
                    AttributeId = Attributes.Value,
                    Value= new DataValue
                    {
                        Value = $"This is comming from the new OpcUA Code {DateTime.Now}"
                    } 
                }
            });

            var resulst = await ompOpcUaClient.WriteAsync(commandCollection, stoppingToken);
            resulst.Switch(
               success =>
               {
                   logger.LogInformation("Write Succeeded: : {results}", success.Select(s => (s.Succeeded, s.Response!.Code)));
               },
               failure =>
               {
                   logger.LogCritical("Write failed");
               });
        }

        private async Task SubscribeToNodes(CancellationToken stoppingToken)
        {
            var commandCollection = new CreateSubscriptionsCommand();
            commandCollection.EndpointUrl = EndPointUrl;
            commandCollection.MonitoredItems.Add(new SubscriptionMonitoredItem
            {
                NodeId = "ns=2;i=1587"
            });

            var resulst = await ompOpcUaClient.CreateSubscriptions(commandCollection, stoppingToken);
            resulst.Switch(
               success =>
               {
                   logger.LogInformation("Subscriptions Succeeded: : {results}", success.Succeeded);
               },
               failure =>
               {
                   logger.LogCritical("Subscriptions failed");
               });
        }
    }
}
