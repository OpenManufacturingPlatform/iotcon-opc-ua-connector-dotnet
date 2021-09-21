using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OMP.Connector.Domain.Configuration;

namespace OMP.Connector.EdgeModule
{
    public class ConfigureConnectorSettings : IConfigureNamedOptions<ConnectorConfiguration>
    {
        private readonly IConfiguration configuration;
        private const string SectionSeparatorKey = ":";
        private const string NativeSettingsKey = "NativeSettings";

        public ConfigureConnectorSettings(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void Configure(string name, ConnectorConfiguration options)
        {
            options.OpcUa?.SetNativeConfig(this.GetOpcUaNativeConfiguration());
            options.Persistance?.SetNativeConfig(this.GetPersistanceNativeConfiguration());
            options.Communication?.Shared.SetNativeConfig(this.GetSharedCommunicationNativeConfiguration());

            this.SetNativeAndSharedCommunicationSettings(options.Communication, options.Communication?.Shared);
        }

        public void Configure(ConnectorConfiguration options)
            => this.Configure(Options.DefaultName, options);

        private void SetNativeAndSharedCommunicationSettings(CommunicationConfiguration communicationConfiguration, SharedConfiguration sharedConfiguration)
        {
            this.SetNativeAndSharedCommunicationSettings(communicationConfiguration?.CommandEndpoint, this.GetCommandEndpointCommunicationNativeConfiguration(), sharedConfiguration);
            this.SetNativeAndSharedCommunicationSettings(communicationConfiguration?.ResponseEndpoint, this.GetResponseEndpointCommunicationNativeConfiguration(), sharedConfiguration);
            this.SetNativeAndSharedCommunicationSettings(communicationConfiguration?.TelemetryEndpoint, this.GetTelemetaryCommunicationNativeConfiguration(), sharedConfiguration);
        }

        private void SetNativeAndSharedCommunicationSettings(CommunicationChannelConfiguration communicationConfiguration, IConfiguration configuration, SharedConfiguration sharedConfiguration)
        {
            if (communicationConfiguration is null)
                return;

            if (communicationConfiguration.Type == sharedConfiguration.Type)
                communicationConfiguration.SetNativeConfig(configuration, sharedConfiguration);
            else
                communicationConfiguration.SetNativeConfig(configuration);
        }

        private IConfiguration GetOpcUaNativeConfiguration()
            => this.configuration.GetSection($"{nameof(ConnectorConfiguration.OpcUa)}{SectionSeparatorKey}{NativeSettingsKey}");

        private IConfiguration GetPersistanceNativeConfiguration()
            => this.configuration.GetSection($"{nameof(ConnectorConfiguration.Persistance)}{SectionSeparatorKey}{NativeSettingsKey}");

        private IConfiguration GetResponseEndpointCommunicationNativeConfiguration()
            => this.configuration.GetSection($"{nameof(ConnectorConfiguration.Communication)}{SectionSeparatorKey}{nameof(ConnectorConfiguration.Communication.ResponseEndpoint)}{SectionSeparatorKey}{NativeSettingsKey}");

        private IConfiguration GetCommandEndpointCommunicationNativeConfiguration()
            => this.configuration.GetSection($"{nameof(ConnectorConfiguration.Communication)}{SectionSeparatorKey}{nameof(ConnectorConfiguration.Communication.CommandEndpoint)}{SectionSeparatorKey}{NativeSettingsKey}");

        private IConfiguration GetSharedCommunicationNativeConfiguration()
            => this.configuration.GetSection($"{nameof(ConnectorConfiguration.Communication)}{SectionSeparatorKey}{nameof(ConnectorConfiguration.Communication.Shared)}{SectionSeparatorKey}{NativeSettingsKey}");

        private IConfiguration GetTelemetaryCommunicationNativeConfiguration()
            => this.configuration.GetSection($"{nameof(ConnectorConfiguration.Communication)}{SectionSeparatorKey}{nameof(ConnectorConfiguration.Communication.TelemetryEndpoint)}{SectionSeparatorKey}{NativeSettingsKey}");
    }
}