using OMP.Connector.Domain.Schema.Messages;
using OMP.Device.Connector.Kafka.Common.Producers;

namespace OMP.Device.Connector.Kafka.ResponsesEndpoint
{
    public interface IResponseProducer : ICustomKafkaProducer<string, CommandResponse> { }
}