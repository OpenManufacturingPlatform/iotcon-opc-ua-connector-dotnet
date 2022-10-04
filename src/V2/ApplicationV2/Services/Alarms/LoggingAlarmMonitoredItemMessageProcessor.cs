// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using Microsoft.Extensions.Logging;
using OMP.PlantConnectivity.OpcUa.Services.Alarms;
using Opc.Ua.Client;

namespace OMP.PlantConnectivity.OpcUa.Services
{
    public class LoggingAlarmMonitoredItemMessageProcessor : IAlarmMonitoredItemMessageProcessor //This is the default OMP implimentations
    {
        private readonly ILogger<LoggingAlarmMonitoredItemMessageProcessor> logger;

        public LoggingAlarmMonitoredItemMessageProcessor(ILogger<LoggingAlarmMonitoredItemMessageProcessor> logger)
        {
            this.logger = logger;
        }

        public string Identifier { get; } = nameof(LoggingAlarmMonitoredItemMessageProcessor);
        public virtual void ProcessMessage(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs eventArguments)
        {
            logger.LogInformation("Message Received: NodeId: {nodeId} | Value: {value}", monitoredItem.StartNodeId, eventArguments.NotificationValue);
        }
    }
}
