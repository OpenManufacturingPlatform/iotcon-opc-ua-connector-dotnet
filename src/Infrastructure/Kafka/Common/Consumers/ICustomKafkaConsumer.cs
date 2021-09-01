using System.Threading;
using Confluent.Kafka;

namespace OMP.Device.Connector.Kafka.Common.Consumers
{
    public interface ICustomKafkaConsumer<TKey, TValue>
    {
        IConsumer<TKey, TValue> Consumer { get; }
        ConsumeResult<TKey, TValue> Consume(CancellationToken cancellationToken = default);
    }
}
