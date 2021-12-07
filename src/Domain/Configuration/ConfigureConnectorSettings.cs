using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace OMP.Connector.Domain.Configuration
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
            options.OpcUa?.SetNativeConfig(GetOpcUaNativeConfiguration());
            options.Persistence?.SetNativeConfig(GetPersistenceNativeConfiguration());
            options.Communication?.Shared.SetNativeConfig(GetSharedCommunicationNativeConfiguration());

            SetNativeAndSharedCommunicationSettings(options.Communication, options.Communication?.Shared);
        }

        public void Configure(ConnectorConfiguration options)
            => Configure(Options.DefaultName, options);

        private void SetNativeAndSharedCommunicationSettings(CommunicationConfiguration communicationConfiguration, SharedConfiguration sharedConfiguration)
        {
            SetNativeAndSharedCommunicationSettings(communicationConfiguration?.CommandEndpoint, GetCommandEndpointCommunicationNativeConfiguration(), sharedConfiguration);
            SetNativeAndSharedCommunicationSettings(communicationConfiguration?.ResponseEndpoint, GetResponseEndpointCommunicationNativeConfiguration(), sharedConfiguration);
            SetNativeAndSharedCommunicationSettings(communicationConfiguration?.TelemetryEndpoint, GetTelemetaryCommunicationNativeConfiguration(), sharedConfiguration);
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
            => configuration.GetSection($"{nameof(ConnectorConfiguration.OpcUa)}{SectionSeparatorKey}{NativeSettingsKey}");

        private IConfiguration GetPersistenceNativeConfiguration()
            => configuration.GetSection($"{nameof(ConnectorConfiguration.Persistence)}{SectionSeparatorKey}{NativeSettingsKey}");

        private IConfiguration GetResponseEndpointCommunicationNativeConfiguration()
            => configuration.GetSection($"{nameof(ConnectorConfiguration.Communication)}{SectionSeparatorKey}{nameof(ConnectorConfiguration.Communication.ResponseEndpoint)}{SectionSeparatorKey}{NativeSettingsKey}");

        private IConfiguration GetCommandEndpointCommunicationNativeConfiguration()
            => configuration.GetSection($"{nameof(ConnectorConfiguration.Communication)}{SectionSeparatorKey}{nameof(ConnectorConfiguration.Communication.CommandEndpoint)}{SectionSeparatorKey}{NativeSettingsKey}");

        private IConfiguration GetSharedCommunicationNativeConfiguration()
            => configuration.GetSection($"{nameof(ConnectorConfiguration.Communication)}{SectionSeparatorKey}{nameof(ConnectorConfiguration.Communication.Shared)}{SectionSeparatorKey}{NativeSettingsKey}");

        private IConfiguration GetTelemetaryCommunicationNativeConfiguration()
            => configuration.GetSection($"{nameof(ConnectorConfiguration.Communication)}{SectionSeparatorKey}{nameof(ConnectorConfiguration.Communication.TelemetryEndpoint)}{SectionSeparatorKey}{NativeSettingsKey}");
    }
}