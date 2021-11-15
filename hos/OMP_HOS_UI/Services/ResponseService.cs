using Microsoft.AspNetCore.SignalR;

namespace OmpHandsOnUi.Services
{
    public class ResponseService : BackgroundService
    {
        private readonly IHubContext<HandsOnHub> hubContext;
        private readonly MqttService mqttService;
        public event EventHandler<ResponseMessage>? OnResponseMessage;
        public ResponseService(IHubContext<HandsOnHub> hubContext, MqttService mqttService)
        {
            this.hubContext = hubContext;
            this.mqttService = mqttService;
            this.mqttService.OnResponseMessage += MqttService_OnResponseMessage;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.mqttService.Start();
            return Task.CompletedTask;
        }

        private void MqttService_OnResponseMessage(object? sender, ResponseMessage responseMessage)
        {
            //hubContext.Clients.All.SendAsync(HandsOnHub.ResponseMessageEventName, responseMessage).GetAwaiter().GetResult();
            OnResponseMessage?.Invoke(this, responseMessage);
        }
    }
}
