// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using System.Threading.Tasks;
using OMP.Connector.Domain.Schema.Messages;

namespace OMP.Connector.Infrastructure.MQTT.Common.Consumers
{
    public interface IMqttCommndListner
    {
        event EventHandler<ErrorEventArgs> OnErrorOccured;

        event EventHandler<CommandRequest> OnMessageReceived;

        Task StartReceivingAsync();

        Task StopReceivingAsync();

        bool IsUpAndRunning();
    }
}