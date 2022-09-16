// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Microsoft.Extensions.Logging;
using Opc.Ua.Client;

namespace OMP.PlantConnectivity.OpcUA.Services.Subscriptions
{
    public class LoggingMonitoredItemMessageProcessor : IMonitoredItemMessageProcessor //This is the default OMP implimentations
    {
        private readonly ILogger<LoggingMonitoredItemMessageProcessor> logger;

        public LoggingMonitoredItemMessageProcessor(ILogger<LoggingMonitoredItemMessageProcessor> logger)
        {
            this.logger = logger;
        }

        public string Identifier { get; } = nameof(LoggingMonitoredItemMessageProcessor);
        public virtual void ProcessMessage(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs eventArguments)
        {
            logger.LogInformation("Message Received: NodeId: {nodeId} | Value: {value}", monitoredItem.StartNodeId, eventArguments.NotificationValue);
        }
    }
}
