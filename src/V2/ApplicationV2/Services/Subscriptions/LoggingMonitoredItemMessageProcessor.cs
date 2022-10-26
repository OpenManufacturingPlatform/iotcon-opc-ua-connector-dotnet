// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Microsoft.Extensions.Logging;
using Opc.Ua.Client;

namespace OMP.PlantConnectivity.OpcUa.Services.Subscriptions
{
    public class LoggingMonitoredItemMessageProcessor : IMonitoredItemMessageProcessor //This is the default OMP implementations
    {
        private readonly ILogger<LoggingMonitoredItemMessageProcessor> logger;

        public LoggingMonitoredItemMessageProcessor(ILogger<LoggingMonitoredItemMessageProcessor> logger)
        {
            this.logger = logger;
        }

        public virtual void ProcessMessage(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs eventArguments)
        {
            logger.LogInformation("Message Received: NodeId: {nodeId} | Value: {value}", monitoredItem.StartNodeId, eventArguments.NotificationValue);
        }
    }
}
