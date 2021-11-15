namespace OmpHandsOnUi.Services
{
    public class ConfigurationService
    {
        public event EventHandler<ConfigurationModel>? OnConfigurationChange;
        public event EventHandler<bool>? OnDemoDataServiceToggled;
        private ConfigurationModel currentConfiguration = new ConfigurationModel();
        public bool DemoDataServiceEnabled {get; private set;}

		public ConfigurationService(IConfiguration configuration)
		{
            configuration.Bind("mqtt", currentConfiguration);
		}

        public void UpdateConfiguration(ConfigurationModel configuration)
        {
            this.OnConfigurationChange?.Invoke(this, configuration);
            this.currentConfiguration = configuration;
        }

        public void ToggleDemoDataService()
        {
            DemoDataServiceEnabled = !DemoDataServiceEnabled;
            this.OnDemoDataServiceToggled?.Invoke(this, DemoDataServiceEnabled);
        }

        public ConfigurationModel GetConfiguration()
        {
            return new ConfigurationModel
            {
                BrokkerAddress = currentConfiguration.BrokkerAddress,
                ResponseTopicName = currentConfiguration.ResponseTopicName,
                TelemetryTopicName = currentConfiguration.TelemetryTopicName,
                BrokkerPort = currentConfiguration.BrokkerPort,
                ClientId = currentConfiguration.ClientId,
                RequestTopicName = currentConfiguration.RequestTopicName
            };
        }
    }
}
