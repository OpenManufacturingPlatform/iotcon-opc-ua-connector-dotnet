// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OMP.Connector.Domain;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Infrastructure.Kafka;
using OMP.Connector.Infrastructure.Kafka.CommandEndpoint;
using OMP.Connector.Infrastructure.Kafka.Common.Consumers;
using OMP.Connector.Infrastructure.Kafka.Common.Events;
using OMP.Connector.Infrastructure.Kafka.Common.Producers;
using OMP.Connector.Infrastructure.Kafka.ConfigurationEndpoint;
using OMP.Connector.Infrastructure.Kafka.Repositories;
using OMP.Connector.Infrastructure.Kafka.Serialization;


namespace OMP.Connector.EdgeModule
{
    internal static class ServiceCollectionExtension
    {
        public static IServiceCollection RegisterDelegateFromService<TService, TDelegate>(
                this IServiceCollection serviceCollection,
                Func<TService, TDelegate> getDelegateFromService)
                where TDelegate : Delegate
        {
            return serviceCollection.AddTransient(serviceProvider =>
                getDelegateFromService(serviceProvider.GetRequiredService<TService>()));
        }

        public static IServiceCollection AddKafkaIntegration(this IServiceCollection serviceCollection)
        {

            serviceCollection.TryAddScoped<IKafkaRequestHandler, KafkaRequestHandler>();
            serviceCollection.TryAddTransient<ISerializerFactory, SerializerFactory>();
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
            serviceCollection.TryAddSingleton<ISubscriptionRepository>(sc => sc.GetRequiredService<KafkaRepository>());
            serviceCollection.TryAddScoped(sc => (IConfigurationPersister)sc.GetRequiredService<IProducerFactory>().CreateConfigurationProducer());

            serviceCollection.AddHostedService<ConfigurationConsumerHostedService>();
            return serviceCollection;
        }
    }
}