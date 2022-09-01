// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OMP.Connector.Domain.Configuration;
using OMP.Connector.Domain.Models.Telemetry;
using OMP.Connector.Domain.OpcUa.Services;
using OMP.Connector.Domain.Schema;
using OMP.Connector.Domain;
using OMP.Connector.Domain.OpcUa;

namespace OMP.Connector.Application.Services
{
    public class OpcAlarmMonitoredItemServiceFactory : IOpcAlarmMonitoredItemServiceFactory
    {
        private ILogger<OpcAlarmMonitoredItemService> logger;
        private IMapper mapper;
        private IOptions<ConnectorConfiguration> connectorConfiguration;
        private IMessageSender messageSender;

        public OpcAlarmMonitoredItemServiceFactory(
            IOptions<ConnectorConfiguration> connectorConfiguration,
            IMessageSender messageSender,
            IMapper mapper,
            ILogger<OpcAlarmMonitoredItemService> logger
            )
        {
            this.logger = logger;
            this.mapper = mapper;
            this.connectorConfiguration = connectorConfiguration;
            this.messageSender = messageSender;
        }

        public IOpcAlarmMonitoredItemService Create(AlarmSubscriptionMonitoredItem alarmMonitoredItemCommand, TelemetryMessageMetadata messageMetadata, IOpcSession opcSession)
        {
            var monitoredItemService = new OpcAlarmMonitoredItemService(connectorConfiguration, messageSender, mapper, logger);
            monitoredItemService.Initialize(alarmMonitoredItemCommand, messageMetadata, opcSession);
            return monitoredItemService;
        }
    }
}
