// SPDX-License-Identifier: MIT. 
// Copyright Contributors to the Open Manufacturing Platform.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OMP.Connector.Application.Repositories;
using OMP.Connector.Domain;
using OMP.Connector.Domain.OpcUa;
using OMP.Connector.Infrastructure.MQTT;
using OMP.Connector.Infrastructure.MQTT.AlarmEndpoint;
using OMP.Connector.Infrastructure.MQTT.CommandEndpoint;
using OMP.Connector.Infrastructure.MQTT.Common;
using OMP.Connector.Infrastructure.MQTT.Common.Consumers;
using OMP.Connector.Infrastructure.MQTT.Common.M2Mqtt;
using OMP.Connector.Infrastructure.MQTT.Common.Publishers;
using OMP.Connector.Infrastructure.MQTT.ResponseEndpoint;
using OMP.Connector.Infrastructure.MQTT.Serialization;
using OMP.Connector.Infrastructure.MQTT.TelemetryEndpoint;

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

        public static IServiceCollection AddMqttIntegration(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ISubscriptionRepository, LocalSubscriptionRepository>();
            serviceCollection.AddSingleton<IAlarmSubscriptionRepository, LocalAlarmSubscriptionRepository>();

            serviceCollection.TryAddScoped<IMqttCommndListner, MqttCommandListner>();

            serviceCollection.TryAddScoped<IMqttClientFactory, M2MqttClientFactory>();
            serviceCollection.TryAddScoped<ISerializer, JsonSerializer>();

            serviceCollection.TryAddScoped<IMqttTelemetryPublisher, MqttTelemetryPublisher>();
            serviceCollection.TryAddScoped<IMqttAlarmPublisher, MqttAlarmPublisher>();
            serviceCollection.TryAddScoped<IMqttResponsePublisher, MqttResponsePublisher>();

            serviceCollection.TryAddScoped<IMqttRequestHandler, MqttRequestHandler>();
            serviceCollection.TryAddScoped<IMessageSender, MqttMessageSender>();
            serviceCollection.AddHostedService<CommandListnerHostedService>();

            return serviceCollection;
        }
    }
}
