
//namespace IoTPE.Connector.Domain
//{
//    public interface IRoutingSettings
//    {
//        string MqttPublisherRouteName { get; set; }
//        string MqttSubscriberRouteName { get; set; }
//        string UpstreamRouteName { get; set; }

//        #region KafkaSettings

//        string KafkaBootstrapServers { get; set; }
//        string KafkaUsername { get; set; }
//        string KafkaKey { get; set; }
//        string KafkaTopicComConDown { get; set; }
//        string KafkaTopicComConUp { get; set; }
//        string KafkaTopicTelemetry { get; set; }
//        string KafkaTopicConfig { get; set; }
//        int KafkaTopicMetadataRefreshIntervalMs { get; set; }
//        string KafkaConsumerGroup { get; set; }
//        #endregion

//        #region MqttSettings

//        string MqttTopicComConUp { get; set; }

//        string MqttTopicComConDown { get; set; }

//        string MqttTopicTelemetry { get; set; }

//        string MqttBrokerUrl { get; set; }

//        int MqttBrokerPort { get; set; }

//        string MqttUserName { get; set; }

//        string MqttUserPassword { get; set; }

//        string MqttClientIdComConUp { get; set; }

//        string MqttClientIdComConDown { get; set; }

//        string MqttClientIdTelemetry { get; set; }

//        byte MqttQoSComConUp { get; set; }

//        byte MqttQoSTelemetry { get; set; }

//        int MaxRetriesComConUp { get; set; }

//        int MaxRetriesTelemetry { get; set; }

//        int RetryWaitSecsComConUp { get; set; }

//        int RetryWaitSecsTelemetry { get; set; }

//        #endregion
//    }
//}