using System.Threading.Tasks;
using Confluent.Kafka;
using OMP.Connector.Domain.Schema.Messages;

namespace OMP.Device.Connector.Kafka
{
    public interface IKafkaRequestHandler
    {
        Task OnMessageReceivedAsync(ConsumeResult<string, CommandRequest> consumeResult);
        void OnMessageReceived(ConsumeResult<string, CommandRequest> consumeResult);
    }
}