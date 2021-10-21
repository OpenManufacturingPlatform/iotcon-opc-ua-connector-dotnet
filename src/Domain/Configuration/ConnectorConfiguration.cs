using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Opc.Ua;

namespace OMP.Connector.Domain.Configuration
{
    public enum CommunicationType
    {
        Kafka = 1,
        MQTT = 2
    }

    public enum PersistanceType
    {
        InMemory = 1,
        Kafka = 2
    }


    public abstract class BaseConnectorConfiguration
    {
        protected IConfiguration Configuration { get; set; }
        public object Settings { get; set; }
        public virtual T GetConfig<T>()
           where T : new()
            => this.Configuration.Get<T>();

        public virtual void SetNativeConfig(IConfiguration configuration)
        {
            this.Configuration = configuration;
            this.Settings = this.ConvertToObject(configuration);
        }

        protected virtual object ConvertToObject(IConfiguration configuration)
        {
            return configuration.GetChildren()
                 .ToDictionary(kv => kv.Key, kv => Convert.ChangeType(kv.Value, typeof(object)))
                 .Aggregate(new ExpandoObject() as IDictionary<string, Object>,
                             (a, p) => { a.Add(p); return a; });
        }
    }

    public abstract class BaseConnectorConfigurationExtendable : BaseConnectorConfiguration
    {
        protected BaseConnectorConfiguration SharedConfiguration { get; set; }
        public object SharedSettings { get; set; }

        public virtual void SetNativeConfig(IConfiguration configuration, BaseConnectorConfiguration sharedConfiguration)
        {
            this.Configuration = configuration;
            this.SharedConfiguration = sharedConfiguration;
            this.Settings = this.ConvertToObject(configuration);
        }

        public override T GetConfig<T>()
        {
            if (this.SharedConfiguration is null)
                return base.GetConfig<T>();

            var value = this.SharedConfiguration.GetConfig<T>();
            this.Configuration.Bind(value);
            return value;
        }
    }

    public sealed class ConnectorConfiguration
    {
        public string ConnectorId { get; set; }
        public OpcUaConfiguration OpcUa { get; set; }
        public bool DisableSubscriptionRestoreService { get; set; }
        public bool EnableMessageFilter { get; set; }
        public CommunicationConfiguration Communication { get; set; }
        public CommunicationChannelConfiguration Persistence { get; set; }
    }

    public sealed class OpcUaConfiguration : BaseConnectorConfiguration
    {
        public bool EnableRegisteredNodes { get; set; }
        public int SubscriptionBatchSize { get; set; }
        public int ReadBatchSize { get; set; }
        public int RegisterNodeBatchSize { get; set; }
        public int AwaitSessionLockTimeoutSeconds { get; set; } = 3;
        public int OperationTimeoutInSeconds { get; set; } = 10;
        public int ReconnectIntervalInSeconds { get; set; }

        public uint NodeMask => (uint)NodeClass.Object |
                                          (uint)NodeClass.Variable |
                                          (uint)NodeClass.Method |
                                          (uint)NodeClass.VariableType |
                                          (uint)NodeClass.ReferenceType |
                                          (uint)NodeClass.Unspecified;
        
        public List<AuthenticationConfiguration> Authentication { get; set; }
    }

    public sealed class AuthenticationConfiguration
    {
        public string Endpoint { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public sealed class CommunicationConfiguration
    {
        public string SchemaUrl { get; set; }
        public SharedConfiguration Shared { get; set; }
        public CommunicationChannelConfiguration TelemetryEndpoint { get; set; }
        public CommunicationChannelConfiguration AlarmEndpoint { get; set; }
        public CommunicationChannelConfiguration CommandEndpoint { get; set; }
        public CommunicationChannelConfiguration ResponseEndpoint { get; set; }
    }

    public class CommunicationChannelConfiguration : BaseConnectorConfigurationExtendable
    {
        public CommunicationType Type { get; set; }
    }

    public sealed class SharedConfiguration : BaseConnectorConfiguration
    {
        public CommunicationType Type { get; set; }
    }
}
