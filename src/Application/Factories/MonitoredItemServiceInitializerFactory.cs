using System;
using Microsoft.Extensions.DependencyInjection;
using OMP.Connector.Domain;
using OMP.Connector.Domain.Models.Telemetry;
using OMP.Connector.Domain.OpcUa.Services;
using OMP.Connector.Domain.Schema;
using Opc.Ua.Client;

namespace OMP.Connector.Application.Factories
{
    public class MonitoredItemServiceInitializerFactory: IMonitoredItemServiceInitializerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public MonitoredItemServiceInitializerFactory(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public MonitoredItem Initialize(SubscriptionMonitoredItem monitoredItem, IComplexTypeSystem complexTypeSystem, TelemetryMessageMetadata telemetryMessageMetadata)
        {
            var opcMonitoredItemService = this._serviceProvider.GetRequiredService<IOpcMonitoredItemService>();
            opcMonitoredItemService.Initialize(monitoredItem, complexTypeSystem , telemetryMessageMetadata);
            return opcMonitoredItemService as MonitoredItem;
        }
    }
}