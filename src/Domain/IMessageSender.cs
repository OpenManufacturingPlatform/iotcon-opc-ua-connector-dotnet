using System.Threading.Tasks;
using OMP.Connector.Domain.Models;
using OMP.Connector.Domain.Schema.Alarms;
using OMP.Connector.Domain.Schema.Messages;
using OMP.Connector.Domain.Schema.SensorTelemetry;

namespace OMP.Connector.Domain
{
    public interface IMessageSender
    {
        Task SendMessageToComConUpAsync(CommandResponse commandResponse);
        Task SendMessageToTelemetryAsync(SensorTelemetryMessage telemetry);
        bool SendMessageToConfig(AppConfigDto configuration);
        Task SendMessageToAlarmsAsync(AlarmMessage alarmMessage);
    }
}