// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System.Collections.Generic;
using System.Threading.Tasks;
using OMP.Connector.Domain.Models;
using OMP.Connector.Domain.Schema.Alarms;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Domain.Schema.SensorTelemetry;

namespace OMP.Connector.Domain
{
    public interface IMessageSender
    {
        Task SendMessageToComConUpAsync(CommandResponse commandResponse, CommandRequest commandRequest = null);
        Task SendMessageToComConUpAsync(IEnumerable<CommandResponse> commandResponse, CommandRequest commandRequest = null);
        Task SendMessageToTelemetryAsync(SensorTelemetryMessage telemetry);
        Task SendMessageToAlarmAsync(AlarmMessage alarm);
        bool SendMessageToConfig(AppConfigDto configuration);
    }
}
