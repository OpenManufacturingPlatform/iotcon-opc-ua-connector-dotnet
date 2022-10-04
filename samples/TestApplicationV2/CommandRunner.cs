// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using OMP.PlantConnectivity.OpcUa;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OMP.PlantConnectivity.OpcUa.Models.Call;
using OMP.PlantConnectivity.OpcUa.Models.Browse;
using OMP.PlantConnectivity.OpcUa.Models.Reads;
using OMP.PlantConnectivity.OpcUa.Models.Subscriptions;
using OMP.PlantConnectivity.OpcUa.Models.Writes;
using Opc.Ua;

namespace TestApplicationV2
{
    internal class CommandRunner : BackgroundService
    {
        const string EndPointUrl = "<SPECIFY_OPCUA_SERVER_ENDPOINT>";
        private const uint NodeMask = (uint)NodeClass.Object |
                                          (uint)NodeClass.Variable |
                                          (uint)NodeClass.Method |
                                          (uint)NodeClass.VariableType |
                                          (uint)NodeClass.ReferenceType |
                                          (uint)NodeClass.Unspecified;
        
        private readonly IOmpOpcUaClient ompOpcUaClient;
        private readonly ILogger<CommandRunner> logger;
        private readonly IHostApplicationLifetime applicationLifetime;

        public CommandRunner(
            IOmpOpcUaClient ompOpcUaClient,
            ILogger<CommandRunner> logger,
            IHostApplicationLifetime applicationLifetime)
        {
            this.ompOpcUaClient = ompOpcUaClient;
            this.logger = logger;
            this.applicationLifetime = applicationLifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await BrowseNodesTest(stoppingToken);
            await BrowseChildNodesTest(stoppingToken);
            //await RemoveAllSubscribeToNodes(stoppingToken);
            //await CallNodesWithoutArguments(stoppingToken);
            //await RunReadNodesTest(stoppingToken);

            //await SubscribeToNodes(stoppingToken);
            //await CallMethodNodeWithArguments(stoppingToken); // This test depends on existence of a valid subscription so that subscription id can be passed in input args.

            //await UnSubscribeFromNodes(stoppingToken);
            
            //await RunReadValuesTest(stoppingToken);
            //await RunWriteTest(stoppingToken);
        }

        private async Task ReadNodesTest(CancellationToken stoppingToken)
        {
            var commandCollection = new ReadNodeCommandCollection(EndPointUrl)
            {
                new ReadNodeCommand("ns=5;i=3"),
                new ReadNodeCommand("ns=5;i=6")
            };

            var results = await ompOpcUaClient.ReadNodesAsync(commandCollection, stoppingToken);
            results.Switch(
                result =>
                {
                    logger.LogInformation("ReadNodes Succeeded: {results}", result.Select(s => (s.Succeeded, s.Response!.DisplayName)));
                },
                exception =>
                {
                    logger.LogCritical("ReadNodes failed");
                });
        }

        // Browse nodes starting from the Objects folder - used for node discovery without having to specify a parent node
        private async Task BrowseNodesTest(CancellationToken stoppingToken)
        {
            var results = await ompOpcUaClient.BrowseNodesAsync(EndPointUrl, 1, stoppingToken);
            results.Switch(
                result =>
                {
                    logger.LogInformation("BrowseNodes Succeeded: {succeeded} | {node}", result.Succeeded, result.Response);
                },
                exception =>
                {
                    logger.LogCritical("BrowseNodes failed");
                });
        }

        // Browse child nodes of specified parent node
        private async Task BrowseChildNodesTest(CancellationToken stoppingToken)
        {
            var brwoseDescription = new BrowseDescription
            {
                NodeId = "ns=4;i=1240", //Server -> Boilers
                BrowseDirection = BrowseDirection.Forward,
                ReferenceTypeId = ReferenceTypeIds.HierarchicalReferences,
                IncludeSubtypes = true,
                NodeClassMask = NodeMask,
                ResultMask = (uint)BrowseResultMask.All
            };
            var command = new BrowseChildNodesCommand(EndPointUrl, brwoseDescription, 2);

            var results = await ompOpcUaClient.BrowseChildNodesAsync(command, stoppingToken);
            results.Switch(
                result =>
                {
                    logger.LogInformation("BrowseChildNodes Succeeded: {succeeded} | {node}", result.Succeeded, result.Response);
                },
                exception =>
                {
                    logger.LogCritical("BrowseChildNodes failed");
                });
        }

        private async Task RunReadValuesTest(CancellationToken stoppingToken)
        {
            var commandCollection = new ReadValueCommandCollection(EndPointUrl)
            {
                new ReadValueCommand("ns=2;i=1585", doRegisteredRead: false),
                new ReadValueCommand("ns=2;i=1587", doRegisteredRead: true)
            };

            var results = await ompOpcUaClient.ReadValuesAsync(commandCollection, stoppingToken);
            results.Switch(
                result =>
                {
                    logger.LogInformation("ReadValues Succeeded: {results}", result.Select(s => (s.Succeeded, s.Response!.Value)));
                },
                exception =>
                {
                    logger.LogCritical("ReadValues failed");
                });
        }

        private async Task RunReadNodesTest(CancellationToken stoppingToken)
        {
            var commandCollection = new ReadNodeCommandCollection(EndPointUrl)
            {
                new ReadNodeCommand("ns=5;i=3"),
                new ReadNodeCommand("ns=5;i=6")
            };

            var results = await ompOpcUaClient.ReadNodesAsync(commandCollection, stoppingToken);
            results.Switch(
                result =>
                {
                    logger.LogInformation("ReadNodes Succeeded: {results}", result.Select(s => (s.Succeeded, s.Response!.DisplayName)));
                },
                exception =>
                {
                    logger.LogCritical("ReadNodes failed");
                });
        }

        private async Task RunWriteTest(CancellationToken stoppingToken)
        {
            var commandCollection = new WriteCommandCollection(EndPointUrl)
            {
                new WriteCommand(new WriteValue
                {
                    NodeId = "ns=2;i=920",
                    AttributeId = Attributes.Value,
                    Value = new DataValue
                    {
                        Value = $"This is comming from the new OpcUa Code {DateTime.Now}"
                    }
                },
                DoRegisteredWrite: false)
            };

            var results = await ompOpcUaClient.WriteAsync(commandCollection, stoppingToken);
            results.Switch(
               result =>
               {
                   logger.LogInformation("Write Succeeded: : {results}", result.Select(s => (s.Succeeded, s.Response!.Code)));
               },
               exception =>
               {
                   logger.LogCritical("Write failed");
               });
        }

        private async Task SubscribeToNodes(CancellationToken stoppingToken)
        {
            var commandCollection = new CreateSubscriptionsCommand(EndPointUrl);
            commandCollection.MonitoredItems.Add(new SubscriptionMonitoredItem
            {
                NodeId = "i=2257"
            });

            var results = await ompOpcUaClient.CreateSubscriptionsAsync(commandCollection, stoppingToken);
            results.Switch(
               result =>
               {
                   logger.LogInformation("Create Subscriptions Succeeded: : {results}", result.Succeeded);
               },
               exception =>
               {
                   logger.LogCritical("Create Subscriptions failed");
               });
        }

        private async Task UnSubscribeFromNodes(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(55));
            var commandCollection = new RemoveSubscriptionsCommand(EndPointUrl, new List<string> { "ns=2;i=1587" });

            var results = await ompOpcUaClient.RemoveSubscriptionsAsync(commandCollection, stoppingToken);
            results.Switch(
               result =>
               {
                   logger.LogInformation("Remove Subscriptions Succeeded: : {results}", result.Succeeded);
               },
               exception =>
               {
                   logger.LogCritical("Remove Subscriptions failed");
               });
        }

        private async Task RemoveAllSubscribeToNodes(CancellationToken stoppingToken)
        {
            var commandCollection = new CreateSubscriptionsCommand(EndPointUrl, new List<SubscriptionMonitoredItem>
            {
                new SubscriptionMonitoredItem { NodeId = "ns=2;i=1580", },
                new SubscriptionMonitoredItem { NodeId = "ns=2;i=1582", },
                new SubscriptionMonitoredItem { NodeId = "ns=2;i=1600", },
                new SubscriptionMonitoredItem { NodeId = "ns=2;i=1599", }
            });

            var createSubscriptionResult = await ompOpcUaClient.CreateSubscriptionsAsync(commandCollection, stoppingToken);
            createSubscriptionResult.Switch(
               result =>
               {
                   logger.LogInformation("Create Subscriptions Succeeded: : {results}", result.Succeeded);
               },
               exception =>
               {
                   logger.LogCritical("Create Subscriptions failed");
               });

            await Task.Delay(TimeSpan.FromSeconds(5));

            await RemoveAllSubscriptions(stoppingToken);
        }

        private async Task RemoveAllSubscriptions(CancellationToken stoppingToken)
        {
            var removeAllCommand = new RemoveAllSubscriptionsCommand(EndPointUrl);
            var removeAllResult = await ompOpcUaClient.RemoveAllSubscriptionsAsync(removeAllCommand, stoppingToken);

            removeAllResult.Switch(
               result =>
               {
                   logger.LogInformation("All Subscriptions Removed: : {results}", result.Succeeded);
               },
               exception =>
               {
                   logger.LogCritical("Remove All Subscriptions failed");
               });
        }

        private async Task CallNodesWithoutArguments(CancellationToken stoppingToken)
        {
            var command = new CallCommandCollection();
            command.EndpointUrl = EndPointUrl;

            var actionLoop = new List<(string Name, string MethodId)>
            {
                ("Start", "ns=5;i=33"),
                ("Suspend", "ns=5;i=34"),
                ("Resume", "ns=5;i=35"),
                ("Halt", "ns=5;i=36"),
                ("Reset", "ns=5;i=37"),
            };

            foreach (var selectedAction in actionLoop)
            {
                command.Add(new CallCommand
                {
                    NodeId = selectedAction.MethodId
                });

                logger.LogInformation("Attempting to {action} Simmulation", selectedAction.Name);

                var callResult = await ompOpcUaClient.CallNodesAsync(command, stoppingToken);
                callResult.Switch(
                   result =>
                   {
                       logger.LogInformation("Call: {action}={results} {methodId}", selectedAction.Name, result.Succeeded, selectedAction.MethodId);
                   },
                   exception =>
                   {
                       logger.LogCritical("Call failed");
                   });

                await Task.Delay(2500);
                command.Clear();
            }
        }

        private async Task CallMethodNodeWithArguments(CancellationToken stoppingToken)
        {
            var methodName = "GetMonitoredItems";
            var methodId = "i=11492";

            var command = new CallCommandCollection();
            command.EndpointUrl = EndPointUrl;

            command.Add(new CallCommand
            {
                NodeId = methodId,
                Arguments = new Dictionary<string, object>()
                {
                    { "SubscriptionId", 1 }
                }
            });

            logger.LogInformation($"Attempting to call {methodName}");

            var callResult = await ompOpcUaClient.CallNodesAsync(command, stoppingToken);
            callResult.Switch(
                result =>
                {
                    logger.LogInformation($"Call: {methodName}={result.Succeeded} {methodId}");
                    result.Response?.Results.First().OutputArguments.ForEach(arg => logger.LogInformation($"Output Argument: {arg.Value}"));
                },
                exception =>
                {
                    logger.LogCritical($"Call to {methodName} failed");
                });

            await Task.Delay(2500);
            command.Clear();
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            // Good practice to clean up OPC UA sessions when stopping the application
            applicationLifetime.ApplicationStopping.Register(async () => await OnStoppingAsync(cancellationToken));

            await ompOpcUaClient.OpenSessionAsync(EndPointUrl, cancellationToken);
            await base.StartAsync(cancellationToken);
        }

        private Task OnStoppingAsync(CancellationToken cancellationToken)
            => ompOpcUaClient.CloseAllActiveSessionsAsync(cancellationToken);
    }
}
