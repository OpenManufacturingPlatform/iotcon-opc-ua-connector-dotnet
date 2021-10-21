using OMP.Connector.Infrastructure.Kafka.AlarmEndpoint;
using OMP.Connector.Infrastructure.Kafka.ConfigurationEndpoint;
using OMP.Connector.Infrastructure.Kafka.ResponsesEndpoint;
using OMP.Connector.Infrastructure.Kafka.TelemetryEndpoint;

namespace OMP.Connector.Infrastructure.Kafka.Common.Producers
{
    public interface IProducerFactory
    {
        IConfigurationProducer CreateConfigurationProducer();
        IResponseProducer CreateResponseProducer();
        ITelemetryProducer CreateTelemetryProducer();
        IAlarmProducer CreateAlarmProducer();
    }
}
