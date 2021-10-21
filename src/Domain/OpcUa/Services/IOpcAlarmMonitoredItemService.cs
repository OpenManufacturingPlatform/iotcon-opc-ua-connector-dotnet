using System;
using OMP.Connector.Domain.Models.Telemetry;
using OMP.Connector.Domain.Schema;
using Opc.Ua.Client;

namespace OMP.Connector.Domain.OpcUa.Services
{
    public interface IOpcAlarmMonitoredItemService : IDisposable
    {
        public void Initialize(AlarmSubscriptionMonitoredItem monitoredItemCommand, IComplexTypeSystem complexTypeSystem, TelemetryMessageMetadata messageMetadata, Session session);

        public TelemetryMessageMetadata MessageMetadata { get; set; }
    }
}