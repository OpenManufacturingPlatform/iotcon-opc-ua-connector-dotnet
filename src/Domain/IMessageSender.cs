using System.Threading.Tasks;
using OMP.Connector.Domain.Models;
using Omp.Connector.Domain.Schema.Messages;
using Omp.Connector.Domain.Schema.SensorTelemetry;

namespace OMP.Connector.Domain
{
    public interface IMessageSender
    {
        Task SendMessageToComConUpAsync(CommandResponse commandResponse);
        Task SendMessageToTelemetryAsync(SensorTelemetryMessage telemetry);
        bool SendMessageToConfig(AppConfigDto configuration);
    }
}