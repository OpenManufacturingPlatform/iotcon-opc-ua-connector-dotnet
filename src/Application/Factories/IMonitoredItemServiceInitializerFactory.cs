using OMP.Connector.Domain;
using OMP.Connector.Domain.Models.Telemetry;
using Omp.Connector.Domain.Schema;
using Opc.Ua.Client;

namespace OMP.Connector.Application.Factories
{
    public interface IMonitoredItemServiceInitializerFactory
    {
        public MonitoredItem Initialize(
            SubscriptionMonitoredItem monitoredItem,
            IComplexTypeSystem complexTypeSystem,
            TelemetryMessageMetadata telemetryMessageMetadata);
    }
}
