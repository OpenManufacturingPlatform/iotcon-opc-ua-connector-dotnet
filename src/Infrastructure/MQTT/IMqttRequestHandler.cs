// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Threading.Tasks;
using OMP.Connector.Domain.Schema.Messages;

namespace OMP.Connector.Infrastructure.MQTT
{
    public interface IMqttRequestHandler
    {
        Task OnMessageReceived(CommandRequest request);
    }
}