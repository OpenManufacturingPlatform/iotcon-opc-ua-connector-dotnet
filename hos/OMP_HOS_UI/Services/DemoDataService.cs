using System.Text;
using Newtonsoft.Json;

namespace OmpHandsOnUi.Services
{
    public class DemoDataService : BackgroundService
    {
        private readonly ConfigurationService configurationService;
        private readonly MqttService mqttService;
        public bool Enabled { get; set; }
        public bool Running { get; private set; }

        public DemoDataService(ConfigurationService configurationService, MqttService mqttService)
        {
            this.configurationService = configurationService;
            this.mqttService = mqttService;
            this.configurationService.OnDemoDataServiceToggled += ConfigurationService_OnDemoDataServiceToggled;
        }

        private void ConfigurationService_OnDemoDataServiceToggled(object? sender, bool e)
            => this.Enabled = e;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            mqttService.Start();
            this.Running = true;
            var telemetary = true;
            while (true)
            {
                if (this.Enabled)
                {
                    if (telemetary)
                        SendTelemetry();
                    else
                        SendResponse();

                    telemetary = !telemetary;
                }
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            this.Running = false;
            return base.StopAsync(cancellationToken);
        }

        private void SendTelemetry()
            => mqttService
            .PublishMessage(
                configurationService.GetConfiguration().TelemetryTopicName,
                $"Telemetry message from {nameof(DemoDataService)} at {DateTime.UtcNow}"
            );

        private void SendResponse()
            => mqttService
            .PublishMessage(
                configurationService.GetConfiguration().ResponseTopicName,
                $"Response message from {nameof(DemoDataService)} at {DateTime.UtcNow}"
            );

        protected virtual byte[] ConvertToBytes<T>(T message)
        {
            var serializedMessage = JsonConvert.SerializeObject(message);
            return Encoding.UTF8.GetBytes(serializedMessage);
        }
    }
}