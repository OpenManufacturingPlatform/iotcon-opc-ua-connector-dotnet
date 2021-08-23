using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OMP.Connector.Domain;
using OMP.Connector.Domain.OpcUa;
using OMP.Device.Connector.Kafka;
using OMP.Device.Connector.Kafka.CommandEndpoint;
using OMP.Device.Connector.Kafka.Common.Consumers;
using OMP.Device.Connector.Kafka.Common.Events;
using OMP.Device.Connector.Kafka.Common.Producers;
using OMP.Device.Connector.Kafka.ConfigurationEndpoint;
using OMP.Device.Connector.Kafka.Repositories;
using OMP.Device.Connector.Kafka.Serialization;

namespace OMP.Connector.EdgeModule.DI
{
    public static class ServiceCollectionExtensionKafka
    {
        public static IServiceCollection AddKafkaIntegration(this IServiceCollection serviceCollection)
        {

            serviceCollection.TryAddScoped<IKafkaRequestHandler, KafkaRequestHandler>();
            serviceCollection.TryAddTransient<ISerializerFactory, JsonSerializerFactory>();
            serviceCollection.TryAddSingleton<IKafkaEventHandlerFactory, DefaultKafkaEventHandlerFactory>();
            serviceCollection.TryAddSingleton<IConsumerFactory, ConsumerFactory>();
            serviceCollection.TryAddScoped<IProducerFactory, ProducerFactory>();
            serviceCollection.TryAddScoped<IMessageSender, KafkaMessageSender>();
            serviceCollection.AddKafkaCommandConsumer()
                             .AddKafkaAppConfigConfiguration();
            return serviceCollection;
        }

        private static IServiceCollection AddKafkaCommandConsumer(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddHostedService<CommandConsumerHostedService>();
            return serviceCollection;
        }

        private static IServiceCollection AddKafkaAppConfigConfiguration(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<KafkaRepository>();
            serviceCollection.TryAddSingleton<IKafkaApplicationConfigurationRepository>(sc => sc.GetRequiredService<KafkaRepository>());
            serviceCollection.TryAddSingleton<IEndpointDescriptionRepository>(sc => sc.GetRequiredService<KafkaRepository>());
            serviceCollection.TryAddSingleton<ISubscriptionRepository>(sc => sc.GetRequiredService<KafkaRepository>());
            serviceCollection.TryAddScoped(sc => (IConfigurationPersister)sc.GetRequiredService<IProducerFactory>().CreateConfigurationProducer());

            serviceCollection.AddHostedService<ConfigurationConsumerHostedService>();
            return serviceCollection;
        }
    }
}