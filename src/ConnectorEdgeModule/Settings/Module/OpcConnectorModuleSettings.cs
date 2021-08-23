using System.ComponentModel.DataAnnotations;
using OMP.Connector.Infrastructure.Kafka;

namespace OMP.Connector.EdgeModule.Settings.Module
{
    public record OpcConnectorModuleSettings : OpcUaSettings//, IOpcUaConnectorSettings, ITelemetrySettings, ISchemaSettings
    {
        #region ITelemetrySettings
        private volatile int _telemetryUploadTimeoutMs = 10000;
        public int TelemetryUploadTimeoutMs => this._telemetryUploadTimeoutMs;
        public void SetTelemetryUploadTimeoutMs(int timeOutMs)
        {
            if (timeOutMs < this._telemetryUploadTimeoutMs)
                this._telemetryUploadTimeoutMs = timeOutMs;
        }

        public bool UseTransientTelemetryPublisher { get; set; } = false;

        #endregion

        #region ISchemaSettings

        [Required] public string SchemaUrl { get; set; }

        #endregion

        #region OpcUaConnectorSettings

        [Required] public string ConnectorId { get; set; }
        public bool EnableMessageFilter { get; set; } = false;
        public bool UseAsyncKafkaProducer { get; set; } = false;
        public bool DisableSubscriptionRestoreService { get; set; } = true;

        #endregion
    }
}