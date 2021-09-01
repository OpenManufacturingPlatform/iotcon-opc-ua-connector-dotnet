using System;
using OMP.Connector.Domain.Models.Telemetry;
using OMP.Connector.Domain.Schema;

namespace OMP.Connector.Domain.OpcUa.Services
{
    public interface IOpcMonitoredItemService: IDisposable
    {
        public void Initialize(SubscriptionMonitoredItem monitoredItemCommand, IComplexTypeSystem complexTypeSystem, TelemetryMessageMetadata messageMetadata);

        public TelemetryMessageMetadata MessageMetadata { get; set; }
    }
}
