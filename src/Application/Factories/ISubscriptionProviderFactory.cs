using OMP.Connector.Domain.Models.Telemetry;
using OMP.Connector.Domain.Providers;
using OMP.Connector.Domain.Schema.Interfaces;

namespace OMP.Connector.Application.Factories
{
    public interface ISubscriptionProviderFactory
    {
        ISubscriptionProvider GetProvider(ICommandRequest command, TelemetryMessageMetadata telemetryMessageMetadata);
    }
}