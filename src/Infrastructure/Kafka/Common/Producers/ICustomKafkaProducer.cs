using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using OMP.Device.Connector.Kafka.Common.Producers.Responses;
using OneOf;

namespace OMP.Device.Connector.Kafka.Common.Producers
{
    public interface ICustomKafkaProducer<TKey, TValue>
    {
        public IProducer<TKey, TValue> Producer { get; }
        Message<TKey, TValue> CreateMessage(TKey key, TValue value, Headers headers = null);
        public Message<TKey, TValue> CreateMessage(TValue value, Headers headers = null);
        public Task<OneOf<PublishSucceeded, PublishPartialSucceeded, PublishedFailedMessageSizeTooLarge, PublishFailed>> ProduceAsync(TValue message, CancellationToken cancellationToken = default);
        public Task<OneOf<PublishSucceeded, PublishPartialSucceeded, PublishedFailedMessageSizeTooLarge, PublishFailed>> ProduceAsync(TKey key, TValue message, CancellationToken cancellationToken = default);
    }
}