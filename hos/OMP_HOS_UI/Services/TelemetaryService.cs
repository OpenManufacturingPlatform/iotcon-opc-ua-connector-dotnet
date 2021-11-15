using Microsoft.AspNetCore.SignalR;

namespace OmpHandsOnUi.Services
{
    public class TelemetaryService : BackgroundService
    {
        private readonly IHubContext<HandsOnHub> hubContext;
        private readonly MqttService mqttService;
        public event EventHandler<TelemetryMessage>? OnTelemetryMessage;
        public TelemetaryService(IHubContext<HandsOnHub> hubContext, MqttService mqttService)
        {
            this.hubContext = hubContext;
            this.mqttService = mqttService;
            this.mqttService.OnTelemetryMessage += MqttService_OnTelemetryMessage;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.mqttService.Start();
            return Task.CompletedTask;
        }

        private void MqttService_OnTelemetryMessage(object? sender, TelemetryMessage telemetryMessage)
        {
            //hubContext.Clients.All.SendAsync(HandsOnHub.TelemetryReceivedEventName, telemetryMessage).GetAwaiter().GetResult();
            OnTelemetryMessage?.Invoke(this, telemetryMessage);
        }
    }
}
