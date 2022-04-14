// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Threading.Tasks;
using Confluent.Kafka;
using OMP.Connector.Domain.Schema.Messages;

namespace OMP.Connector.Infrastructure.Kafka
{
    public interface IKafkaRequestHandler
    {
        Task OnMessageReceivedAsync(ConsumeResult<string, CommandRequest> consumeResult);
        void OnMessageReceived(ConsumeResult<string, CommandRequest> consumeResult);
    }
}