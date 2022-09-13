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

namespace OMP.Connector.Application.Services
{
    public class OpcMonitoredItemServiceFactory : IOpcMonitoredItemServiceFactory
    {
        private ILogger<OpcMonitoredItemService> logger;
        private IMapper mapper;
        private IOptions<ConnectorConfiguration> connectorConfiguration;
        private IMessageSender messageSender;

        public OpcMonitoredItemServiceFactory(
            IOptions<ConnectorConfiguration> connectorConfiguration,
            IMessageSender messageSender,
            IMapper mapper,
            ILogger<OpcMonitoredItemService> logger
            )
        {
            this.logger = logger;
            this.mapper = mapper;
            this.connectorConfiguration = connectorConfiguration;
            this.messageSender = messageSender;
        }

        public IOpcMonitoredItemService Create(SubscriptionMonitoredItem monitoredItemCommand, IComplexTypeSystem complexTypeSystem, TelemetryMessageMetadata messageMetadata)
        {
            var monitoredItemService = new OpcMonitoredItemService(connectorConfiguration, messageSender, mapper, logger);
            monitoredItemService.Initialize(monitoredItemCommand, complexTypeSystem, messageMetadata);
            return monitoredItemService;
        }
    }
}
