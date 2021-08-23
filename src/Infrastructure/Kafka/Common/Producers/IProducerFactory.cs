using OMP.Device.Connector.Kafka.ConfigurationEndpoint;
using OMP.Device.Connector.Kafka.ResponsesEndpoint;
using OMP.Device.Connector.Kafka.TelemetryEndpoint;

namespace OMP.Device.Connector.Kafka.Common.Producers
{
    public interface IProducerFactory
    {
        IConfigurationProducer CreateConfigurationProducer();
        IResponseProducer CreateResponseProducer();
        ITelemetryProducer CreateTelemetryProducer();
    }
}
