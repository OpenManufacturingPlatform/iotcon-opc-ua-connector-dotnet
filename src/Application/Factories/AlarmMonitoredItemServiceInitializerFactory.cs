// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using Microsoft.Extensions.DependencyInjection;
using OMP.Connector.Domain;
using OMP.Connector.Domain.Models.Telemetry;
using OMP.Connector.Domain.OpcUa.Services;
using OMP.Connector.Domain.Schema;
using Opc.Ua.Client;

namespace OMP.Connector.Application.Factories
{
    public class AlarmMonitoredItemServiceInitializerFactory : IAlarmMonitoredItemServiceInitializerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public AlarmMonitoredItemServiceInitializerFactory(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public MonitoredItem Initialize(AlarmSubscriptionMonitoredItem monitoredItem, IComplexTypeSystem complexTypeSystem, TelemetryMessageMetadata telemetryMessageMetadata, Session session)
        {
            var opcMonitoredItemService = this._serviceProvider.GetRequiredService<IOpcAlarmMonitoredItemService>();
            opcMonitoredItemService.Initialize(monitoredItem, complexTypeSystem, telemetryMessageMetadata, session);
            return opcMonitoredItemService as MonitoredItem;
        }
    }
}
