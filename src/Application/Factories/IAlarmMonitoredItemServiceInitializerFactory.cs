using OMP.Connector.Domain;
using OMP.Connector.Domain.Models.Telemetry;
using OMP.Connector.Domain.Schema;
using Opc.Ua.Client;

namespace OMP.Connector.Application.Factories
{
    public interface IAlarmMonitoredItemServiceInitializerFactory
    {
        public MonitoredItem Initialize(
            AlarmSubscriptionMonitoredItem monitoredItem,
            IComplexTypeSystem complexTypeSystem,
            TelemetryMessageMetadata telemetryMessageMetadata,
            Session session);
    }
}
