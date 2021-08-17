//namespace IoTPE.Connector.Domain.Settings
//{
//    public class ModuleBaseSettings : BaseSettings, IRoutingSettings
//    {
//        #region IRoutingSettings
//        public virtual string MqttPublisherRouteName { get; set; }
//        public virtual string MqttSubscriberRouteName { get; set; }
//        public virtual string UpstreamRouteName { get; set; }
//        public virtual string KafkaBootstrapServers { get; set; }
//        public virtual string KafkaUsername { get; set; }
//        public virtual string KafkaKey { get; set; }
//        public virtual string KafkaTopicComConDown { get; set; }
//        public virtual string KafkaTopicComConUp { get; set; }
//        public virtual string KafkaTopicTelemetry { get; set; }
//        public virtual string KafkaTopicConfig { get; set; }
//        public virtual int KafkaTopicMetadataRefreshIntervalMs { get; set; }
//        public virtual string KafkaConsumerGroup { get; set; }
//        public virtual string MqttTopicComConUp { get; set; }
//        public virtual string MqttTopicComConDown { get; set; }
//        public virtual string MqttTopicTelemetry { get; set; }
//        public virtual string MqttBrokerUrl { get; set; }
//        public virtual int MqttBrokerPort { get; set; }
//        public virtual string MqttUserName { get; set; }
//        public virtual string MqttUserPassword { get; set; }
//        public virtual string MqttClientIdComConUp { get; set; }
//        public virtual string MqttClientIdComConDown { get; set; }
//        public virtual string MqttClientIdTelemetry { get; set; }
//        public virtual byte MqttQoSComConUp { get; set; }
//        public virtual byte MqttQoSTelemetry { get; set; }
//        public virtual int MaxRetriesComConUp { get; set; } = 5;
//        public virtual int MaxRetriesTelemetry { get; set; } = 0;
//        public virtual int RetryWaitSecsComConUp { get; set; } = 3;
//        public virtual int RetryWaitSecsTelemetry { get; set; } = 3;
//        #endregion
//    }
//}
