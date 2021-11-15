namespace OmpHandsOnUi.Services
{
    public class RequestService
    {
        private readonly MqttService mqttService;

        public RequestService(MqttService mqttService)
        {
            this.mqttService = mqttService;
        }

        public void SendRequest(string requestMessage)
        {
            mqttService.PublishRequest(requestMessage);
        }
    }
}
