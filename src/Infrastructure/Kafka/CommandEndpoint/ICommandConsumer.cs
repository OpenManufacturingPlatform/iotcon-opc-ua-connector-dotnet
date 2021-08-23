using OMP.Connector.Domain.Schema.Messages;
using OMP.Device.Connector.Kafka.Common.Consumers;

namespace OMP.Device.Connector.Kafka.CommandEndpoint
{
    public interface ICommandConsumer: ICustomKafkaConsumer<string, CommandRequest>
    {

    }
}
