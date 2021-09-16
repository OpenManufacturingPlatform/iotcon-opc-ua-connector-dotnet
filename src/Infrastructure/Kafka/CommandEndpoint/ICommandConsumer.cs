using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Infrastructure.Kafka.Common.Consumers;

namespace OMP.Connector.Infrastructure.Kafka.CommandEndpoint
{
    public interface ICommandConsumer : ICustomKafkaConsumer<string, CommandRequest>
    {

    }
}
